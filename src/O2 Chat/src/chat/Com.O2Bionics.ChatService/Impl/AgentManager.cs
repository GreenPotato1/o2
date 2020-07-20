using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.ChatService.Objects.ChatEvents;
using Com.O2Bionics.ChatService.Properties;
using Com.O2Bionics.Utils;
using log4net;

namespace Com.O2Bionics.ChatService.Impl
{
    public class AgentManager : IAgentManager
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(AgentManager));

        private readonly ConcurrentDictionary<Guid, Timer> m_sessionDisconnectTimers
            = new ConcurrentDictionary<Guid, Timer>();

        private readonly ISubscriptionManager m_subscriptionManager;
        private readonly IChatSessionManager m_chatSessionManager;
        private readonly ISettingsStorage m_settingsStorage;
        private readonly IUserStorage m_userStorage;
        private readonly IDepartmentStorage m_departmentStorage;
        private readonly IChatSessionStorage m_chatSessionStorage;
        private readonly IAgentSessionStorage m_agentSessionStorage;
        private readonly INowProvider m_nowProvider;
        private readonly IChatDatabaseFactory m_databaseFactory;

        private long m_stopping;

        public AgentManager(
            ISubscriptionManager subscriptionManager,
            IChatSessionManager chatSessionManager,
            ISettingsStorage settingsStorage,
            IUserStorage userStorage,
            IDepartmentStorage departmentStorage,
            IChatSessionStorage chatSessionStorage,
            IAgentSessionStorage agentSessionStorage,
            INowProvider nowProvider,
            IChatDatabaseFactory databaseFactory)
        {
            m_subscriptionManager = subscriptionManager;
            m_settingsStorage = settingsStorage;
            m_userStorage = userStorage;
            m_nowProvider = nowProvider;
            m_chatSessionManager = chatSessionManager;
            m_departmentStorage = departmentStorage;
            m_agentSessionStorage = agentSessionStorage;
            m_chatSessionStorage = chatSessionStorage;
            m_databaseFactory = databaseFactory;
        }

        public void Start()
        {
            var sessionConnections = m_subscriptionManager.AgentEventSubscribers
                .Call(s => s.GetAgentSessionConnections())
                .Where(x => !ReferenceEquals(x, null))
                .SelectMany(x => x)
                .ToList();
            m_agentSessionStorage.AddConnectedSessions(sessionConnections);
        }

        // wcf service is stopped at this point
        public void Stop()
        {
            m_log.Info("stopping");
            Interlocked.Increment(ref m_stopping);

            foreach (var disconnectTimerEntry in m_sessionDisconnectTimers)
            {
                disconnectTimerEntry.Value.Dispose();
            }
        }

        public List<DepartmentInfo> GetCustomerDepartmentsInfo(ChatDatabase db, uint customerId, bool skipPrivate)
        {
            m_log.DebugFormat("GetCustomerDepartmentsInfo customer={0}", customerId);

            return m_departmentStorage.GetAll(db, customerId, skipPrivate)
                .Select(x => x.AsInfo())
                .ToList();
        }

        public HashSet<uint> GetCustomerOnlineDepartments(uint customerId)
        {
            m_log.DebugFormat("GetCustomerOnlineDepartments customer={0}", customerId);

            HashSet<uint> value;
            return m_onlineDepartments.TryGetValue(customerId, out value) ? value : new HashSet<uint>();
        }

        public HashSet<uint> GetCustomerOnlineAgents(uint customerId)
        {
            m_log.DebugFormat("GetCustomerOnlineAgents customer={0}", customerId);

            HashSet<uint> value;
            return m_onlineAgents.TryGetValue(customerId, out value) ? value : new HashSet<uint>();
        }

        public HashSet<uint> GetAgentDepartmentIds(ChatDatabase db, uint customerId, uint agentId)
        {
            m_log.DebugFormat("GetAgentDepartmentIds agent={0}", agentId);

            var agent = m_userStorage.Get(db, customerId, agentId);
            if (agent == null)
                throw new InvalidOperationException(string.Format(Resources.UserNotFoundError1, agentId));

            return agent.AgentDepartmentIds;
        }

        public AgentSession GetAgentSession(Guid agentSessionGuid)
        {
            m_log.DebugFormat("GetAgentSession agent session={0}", agentSessionGuid);

            return m_agentSessionStorage.Get(agentSessionGuid);
        }

        #region agent session status management

        public AgentSessionConnectResult Connect(ChatDatabase db, Guid agentSessionGuid, uint customerId, uint agentId, string connectionId)
        {
            m_log.DebugFormat("Connect session={0} customer={1} agent={2}", agentSessionGuid, customerId, agentId);

            var agent = m_userStorage.Get(db, customerId, agentId);
            if (agent == null)
                throw new InvalidOperationException(string.Format(Resources.UserNotFoundError1, agentId));

            var session = m_agentSessionStorage.GetOrCreate(agentSessionGuid, customerId, agentId);

            Timer disconnectTimer;
            if (m_sessionDisconnectTimers.TryRemove(session.Guid, out disconnectTimer))
                disconnectTimer.Dispose();

            ChangeStatusAndNotify(
                db,
                customerId,
                agentId,
                _ => m_agentSessionStorage.AddConnection(session.Guid, connectionId));

            return new AgentSessionConnectResult
                {
                    AgentId = agentId,
                    CustomerId = customerId,
                    Departments = new HashSet<uint>(agent.AgentDepartmentIds),
                };
        }

        public void Disconnect(uint customerId, Guid agentSessionGuid, uint agentId, string connectionId)
        {
            m_log.DebugFormat("Disconnect agent session={0}", agentSessionGuid);

            var session = m_agentSessionStorage.Get(agentSessionGuid);
            if (session == null)
            {
                m_log.WarnFormat("Disconnect: Agent session not found. guid={0}", agentSessionGuid);
                return;
            }

            Timer disconnectTimer;
            if (m_sessionDisconnectTimers.TryRemove(session.Guid, out disconnectTimer))
            {
                disconnectTimer.Dispose();
                m_log.WarnFormat("Disconnect: previous disconnect timer has been disposed.");
            }

            var timeout = m_settingsStorage.GetServiceSettings().AgentSessionDisconnectTimeout;
            m_log.DebugFormat(
                "Disconnect: scheduling session {0} in {1} because of disconnected connection {2}",
                agentSessionGuid,
                timeout,
                connectionId);
            var timer = new Timer(
                _ => SessionDisconnect(customerId, session, connectionId),
                null,
                timeout,
                TimeSpan.FromMilliseconds(-1));
            if (!m_sessionDisconnectTimers.TryAdd(agentSessionGuid, timer))
            {
                m_log.Error($"Can't add disconnect timer for the agent {agentId} session {agentSessionGuid}.");
                timer.Dispose();
            }
        }

        private void SessionDisconnect(uint customerId, AgentSession session, string connectionId)
        {
            try
            {
                var stopping = Interlocked.Read(ref m_stopping);
                if (stopping > 0) return;

                m_log.DebugFormat("SessionDisconnect(session={0})", session.Guid);

                Timer disconnectTimer;
                if (m_sessionDisconnectTimers.TryRemove(session.Guid, out disconnectTimer))
                    disconnectTimer.Dispose();

                m_chatSessionStorage.GetAgentSessions(session.CustomerId, session.AgentId);

                var chatSessions = m_chatSessionManager.GetAgentSessions(customerId, session.AgentId);
                var mediaCallSessions = chatSessions
                    .Where(x => x.MediaCallAgentConnectionId == connectionId)
                    .ToList();
                foreach (var mediaCallSession in mediaCallSessions)
                {
                    var chatEvent = new AgentStoppedMediaCallChatEvent(
                        m_nowProvider.UtcNow,
                        "Media call has been stopped because of the Agent disconnect.",
                        session.AgentId);
                    m_chatSessionManager.AddEvent(customerId, mediaCallSession.Skey, chatEvent);
                }

                m_databaseFactory.Query(
                    db =>
                        ChangeStatusAndNotify(
                            db,
                            session.CustomerId,
                            session.AgentId,
                            _ => m_agentSessionStorage.RemoveConnection(session.Guid, connectionId)));
            }
            catch (Exception e)
            {
                m_log.Error($"Exception while processing agent session {session.Guid} disconnect.", e);
            }
        }

        public void SetUserOnlineStatus(ChatDatabase db, uint customerId, Guid agentSessionGuid, bool isOnline)
        {
            m_log.DebugFormat("SetStatus agent session={0}, new status={1}", agentSessionGuid, isOnline);

            var session = m_agentSessionStorage.Get(agentSessionGuid);
            if (session == null)
                throw new InvalidOperationException("Session not found skey=" + agentSessionGuid);

            ChangeStatusAndNotify(
                db,
                session.CustomerId,
                session.AgentId,
                db1 => m_userStorage.Update(db1, customerId, session.AgentId, new User.UpdateInfo { IsOnline = isOnline }, true));
        }

        public void DisconnectAll()
        {
            m_agentSessionStorage.DisconnectAll();
            SetAllOffline();
        }

        private readonly ConcurrentDictionary<uint, HashSet<uint>> m_onlineDepartments =
            new ConcurrentDictionary<uint, HashSet<uint>>();

        private readonly ConcurrentDictionary<uint, HashSet<uint>> m_onlineAgents =
            new ConcurrentDictionary<uint, HashSet<uint>>();

        private void SetAllOffline()
        {
            m_onlineDepartments.Clear();
            m_onlineAgents.Clear();
        }

        private void SetOnline(uint customerId, HashSet<uint> departments, HashSet<uint> agents)
        {
            m_onlineAgents.AddOrUpdate(customerId, agents, (x, y) => agents);
            m_onlineDepartments.AddOrUpdate(customerId, departments, (x, y) => departments);
        }

        private void ChangeStatusAndNotify(ChatDatabase db, uint customerId, uint userId, Action<ChatDatabase> statusChanger)
        {
            var onlineUserIds = m_userStorage.GetOnline(db, customerId);
            onlineUserIds.IntersectWith(m_agentSessionStorage.GetConnectedUsers(customerId));
            var onlineDepartmentIds = m_userStorage.GetCustomerAgentsDepartments(db, customerId, onlineUserIds);

            statusChanger(db);

            var newOnlineUserIds = m_userStorage.GetOnline(db, customerId);
            newOnlineUserIds.IntersectWith(m_agentSessionStorage.GetConnectedUsers(customerId));
            var newOnlineDepartmentIds = m_userStorage.GetCustomerAgentsDepartments(db, customerId, newOnlineUserIds);

            m_log.DebugFormat(
                "ChangeStatus {0}: before(a:{1}, d:{2}), after(a:{3}, d:{4})",
                customerId,
                string.Join(",", onlineUserIds.OrderBy(x => x)),
                string.Join(",", onlineDepartmentIds.OrderBy(x => x)),
                string.Join(",", newOnlineUserIds.OrderBy(x => x)),
                string.Join(",", newOnlineDepartmentIds.OrderBy(x => x)));

            SetOnline(customerId, newOnlineDepartmentIds, newOnlineUserIds);

            var isUserOnline = newOnlineUserIds.Contains(userId);
            if (isUserOnline ^ onlineUserIds.Contains(userId))
            {
                var status = new OnlineStatusInfo { Id = userId, IsOnline = isUserOnline };
                m_subscriptionManager.AgentEventSubscribers
                    .Publish(s => s.AgentStateChanged(customerId, status));
            }

            onlineDepartmentIds.SymmetricExceptWith(newOnlineDepartmentIds);
            if (onlineDepartmentIds.Any())
            {
                var changedDepartmentStatus = onlineDepartmentIds
                    .Select(x => new OnlineStatusInfo { Id = x, IsOnline = newOnlineDepartmentIds.Contains(x) })
                    .ToList();
                m_subscriptionManager.AgentEventSubscribers
                    .Publish(s => s.DepartmentStateChanged(customerId, changedDepartmentStatus));

                var publicDepartments = m_departmentStorage.GetPublicIds(db, customerId, onlineDepartmentIds);
                var publicChangedDepartmentStatus = changedDepartmentStatus
                    .Where(x => publicDepartments.Contains(x.Id))
                    .ToList();
                if (publicChangedDepartmentStatus.Any())
                    m_subscriptionManager.VisitorEventSubscribers
                        .Publish(s => s.DepartmentStateChanged(customerId, publicChangedDepartmentStatus));
            }
        }

        #endregion
    }
}