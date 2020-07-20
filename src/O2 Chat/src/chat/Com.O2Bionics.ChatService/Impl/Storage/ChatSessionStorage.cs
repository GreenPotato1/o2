using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.ChatService.Objects.ChatEvents;
using Com.O2Bionics.Utils;
using log4net;

// ReSharper disable InconsistentlySynchronizedField

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public class ChatSessionStorage : IChatSessionStorage
    {
        private class StorageEntry
        {
            public StorageEntry(ChatSession original, ChatSession changed = null)
            {
                var isNewObject = ReferenceEquals(original, null);
                if (isNewObject && ReferenceEquals(changed, null))
                    throw new InvalidOperationException("original and update can't be null simultaneously");
                m_original = original;
                m_changed = changed;
                IsDirty = isNewObject;
            }

            public bool IsDirty { get; set; }

            private ChatSession m_original;
            private ChatSession m_changed;

            public long Skey => (m_changed ?? m_original).Skey;

            public ChatSession ObjectCopy()
            {
                return new ChatSession(Object());
            }

            public ChatSession Object()
            {
                return m_changed ?? m_original;
            }

            public ChatSession ChangedObject()
            {
                return m_changed ?? (m_changed = new ChatSession(m_original));
            }

            public void ApplyDbUpdateResult(DbUpdate<ChatSession> dbUpdate)
            {
                m_original = dbUpdate.Changed;

                IsDirty =
                    m_original.Status != m_changed.Status
                    || m_original.Events.Count != m_changed.Events.Count;

                if (!IsDirty) m_changed = null;
            }

            public DbUpdate<ChatSession> GetDbUpdate()
            {
                return new DbUpdate<ChatSession>(m_original, new ChatSession(m_changed));
            }
        }


        private static readonly ILog m_log = LogManager.GetLogger(typeof(ChatSessionStorage));

        private readonly object m_lock = new object();

        private readonly LinkedList<StorageEntry> m_list = new LinkedList<StorageEntry>();

        private readonly Dictionary<decimal, LinkedListNode<StorageEntry>> m_entryBySkey
            = new Dictionary<decimal, LinkedListNode<StorageEntry>>();

        private readonly Dictionary<ulong, LinkedListNode<StorageEntry>> m_onlineSessionsByVisitor
            = new Dictionary<ulong, LinkedListNode<StorageEntry>>();

        private long m_lastSessionSkey;
        private long m_lastEventSkey;


        private readonly INowProvider m_nowProvider;
        private readonly IObjectResolver m_objectResolver;

        public ChatSessionStorage(
            IObjectResolver objectResolver,
            INowProvider nowProvider)
        {
            m_nowProvider = nowProvider;
            m_objectResolver = objectResolver;
        }

        public void Load(ChatDatabase db)
        {
            m_lastSessionSkey = db.CHAT_SESSION.Any()
                ? (long)db.CHAT_SESSION.Max(x => x.CHAT_SESSION_ID)
                : 0;
            m_lastEventSkey = db.CHAT_EVENT.Any()
                ? (long)db.CHAT_EVENT.Max(x => x.CHAT_EVENT_ID)
                : 0;

            m_log.DebugFormat("last session skey: {0}, last event skey: {1}", m_lastEventSkey, m_lastEventSkey);

            var sessions = db.CHAT_SESSION
                // TODO: should not load sessions older then configured TimeSpan from now (by last event timestamp)
                // TODO: add old sessions filter here
                .ToDictionary(x => x.CHAT_SESSION_ID);

            foreach (var sessionsSection in sessions.Values.Section(1000).Select(x => x.ToList()).ToList())
            {
// ReSharper disable once AccessToForEachVariableInClosure
                var idList = sessionsSection.Select(x => x.CHAT_SESSION_ID).ToList();
                var events = db.CHAT_EVENT
                    .Where(x => idList.Contains(x.CHAT_SESSION_ID))
                    .OrderBy(x => x.CHAT_EVENT_ID)
                    .ToLookup(x => x.CHAT_SESSION_ID);
                foreach (var session in sessionsSection)
                {
                    var bo = new ChatSession(session, events[session.CHAT_SESSION_ID], m_objectResolver);
                    var entry = new StorageEntry(bo);
                    AddEntry(entry);
                }
            }
        }

        #region read access

        public int Count()
        {
            lock (m_lock)
            {
                return m_list.Count;
            }
        }

        public ChatSession Get(uint customerId, long skey)
        {
            m_log.DebugFormat("get {0}", skey);

            StorageEntry entry;
            lock (m_lock)
                entry = LookupBySkey(skey);

            if (entry == null) return null;
            lock (entry)
                return entry.ObjectCopy();
        }

        public ChatSession GetVisitorOnlineSession(uint customerId, ulong visitorId)
        {
            m_log.DebugFormat("get active session for visitor id={0}", visitorId);

            StorageEntry entry;
            lock (m_lock)
                entry = LookupOnlineSessionByVisitorId(visitorId);

            if (entry == null) return null;
            lock (entry)
                return entry.ObjectCopy();
        }

        public List<ChatSession> GetAgentVisibleSessions(uint customerId, uint agentId, HashSet<uint> departmentIds)
        {
            List<StorageEntry> entries;

            lock (m_lock)
            {
                entries = m_list
                    .Select(x => new { x, obj = x.Object() })
                    .Where(x => x.obj.Status == ChatSessionStatus.Queued || x.obj.Status == ChatSessionStatus.Active)
                    .Where(
                        x => x.obj.Agents.Any(y => y.AgentId == agentId)
                             || x.obj.Invites
                                 .OfType<ChatSessionAgentInvite>()
                                 .Any(y => y.IsPending && y.AgentId == agentId)
                             || x.obj.Invites
                                 .OfType<ChatSessionDepartmentInvite>()
                                 .Any(y => y.IsPending && departmentIds.Contains(y.DepartmentId)))
                    .Select(x => x.x)
                    .OrderBy(x => x.Skey)
                    .ToList();
            }

            return entries
                .Select(
                    x =>
                        {
                            lock (x) return x.ObjectCopy();
                        })
                .ToList();
        }

        public List<ChatSession> GetAgentSessions(uint customerId, uint agentId)
        {
            List<StorageEntry> entries;
            lock (m_lock)
            {
                entries = m_list
                    .Select(x => new { x, obj = x.Object() })
                    .Where(x => x.obj.Status == ChatSessionStatus.Queued || x.obj.Status == ChatSessionStatus.Active)
                    .Where(
                        x => x.obj.Agents.Any(y => y.AgentId == agentId)
                             || x.obj.Invites
                                 .OfType<ChatSessionAgentInvite>()
                                 .Any(y => y.IsPending && y.AgentId == agentId))
                    .Select(x => x.x)
                    .OrderBy(x => x.Skey)
                    .ToList();
            }

            return entries
                .Select(
                    x =>
                        {
                            lock (x) return x.ObjectCopy();
                        })
                .ToList();
        }

        #endregion

        #region List management. All operations should be locked.

        private StorageEntry LookupBySkey(long skey)
        {
            LinkedListNode<StorageEntry> node;
            return m_entryBySkey.TryGetValue(skey, out node) ? MakeFirst(node) : null;
        }

        private StorageEntry LookupOnlineSessionByVisitorId(ulong id)
        {
            LinkedListNode<StorageEntry> node;
            return m_onlineSessionsByVisitor.TryGetValue(id, out node) ? MakeFirst(node) : null;
        }

        private StorageEntry MakeFirst(LinkedListNode<StorageEntry> node)
        {
            m_list.Remove(node);
            m_list.AddFirst(node);
            return node.Value;
        }

        private void AddEntry(StorageEntry entry)
        {
            m_log.DebugFormat("add {0}", entry.Skey);

            var node = m_list.AddFirst(entry);

            var obj = entry.Object();
            m_entryBySkey[obj.Skey] = node;
            if (obj.VisitorId.HasValue && obj.Status != ChatSessionStatus.Completed && !obj.IsOffline)
                m_onlineSessionsByVisitor[obj.VisitorId.Value] = node;
        }

        private void RemoveNode(LinkedListNode<StorageEntry> node)
        {
            var obj = node.Value.Object();
            m_entryBySkey.Remove(obj.Skey);
            if (!obj.IsOffline && obj.VisitorId.HasValue)
                m_onlineSessionsByVisitor.Remove(obj.VisitorId.Value);

            m_list.Remove(node);
        }

        #endregion

        #region Session updates

        // creates new session initiated by agent
        public ChatSession CreateNew(uint customerId, ChatEventBase chatEvent, bool isOffline, ulong? visitorId)
        {
            m_log.DebugFormat("creating new session with. cid={0} offlie={1}, vid={2}", customerId, isOffline, visitorId);

            var now = m_nowProvider.UtcNow;
            var session = new ChatSession(GetNewSessionSkey(), customerId, visitorId, isOffline, now);

            chatEvent.AssignId(GetNewSessionEventSkey());
            chatEvent.Apply(session, m_objectResolver);
            session.Add(chatEvent);

            var entry = new StorageEntry(null, session);
            var copy = entry.ObjectCopy();

            lock (m_lock)
                AddEntry(entry);
            return copy;
        }


        // adds agent event. fails if no session found.
        public ChatSession AddEvent(uint customerId, long skey, ChatEventBase chatEvent)
        {
            m_log.DebugFormat("adding event to session {0}", skey);

            StorageEntry entry;
            lock (m_lock)
                entry = LookupBySkey(skey);

            if (entry == null)
                throw new InvalidOperationException("Session not found by skey=" + skey);

            chatEvent.AssignId(GetNewSessionEventSkey());

            // TODO: review locking
            lock (m_lock)
            lock (entry)
            {
                var obj = entry.ChangedObject();
                chatEvent.Apply(obj, m_objectResolver);
                obj.Add(chatEvent);
                entry.IsDirty = true;

                if (obj.VisitorId.HasValue && obj.Status == ChatSessionStatus.Completed && !obj.IsOffline)
                    m_onlineSessionsByVisitor.Remove(obj.VisitorId.Value);

                return entry.ObjectCopy();
            }
        }

        // adds visitor event to the visitor's online session. creates new session if can't find one.
        public ChatSession AddVisitorOnlineEvent(uint customerId, ulong visitorId, ChatEventBase chatEvent)
        {
            m_log.DebugFormat("adding event to visitor {0} online session", visitorId);

            StorageEntry entry;
            lock (m_lock)
                entry = LookupOnlineSessionByVisitorId(visitorId);

            if (entry == null)
            {
                m_log.DebugFormat("creating new online session for visitor {0}", visitorId);

                var now = m_nowProvider.UtcNow;
                var session = new ChatSession(GetNewSessionSkey(), customerId, visitorId, false, now);
                entry = new StorageEntry(null, session);

                lock (m_lock)
                {
                    var entry2 = LookupOnlineSessionByVisitorId(visitorId);
                    if (entry2 == null)
                        AddEntry(entry);
                    entry = entry2 ?? entry;
                }
            }

            chatEvent.AssignId(GetNewSessionEventSkey());

            // TODO: review locking
            lock (m_lock)
            lock (entry)
            {
                var session = entry.ChangedObject();
                chatEvent.Apply(session, m_objectResolver);
                session.Add(chatEvent);
                entry.IsDirty = true;

                if (session.Status == ChatSessionStatus.Completed)
                    m_onlineSessionsByVisitor.Remove(visitorId);

                return entry.ObjectCopy();
            }
        }

        #endregion

        #region IDbUpdaterStorage<>

        public List<DbUpdate<ChatSession>> GetDbUpdates()
        {
            var updates = new List<DbUpdate<ChatSession>>();
            lock (m_lock)
            {
                foreach (var entry in m_list)
                {
                    lock (entry)
                    {
                        if (entry.IsDirty) updates.Add(entry.GetDbUpdate());
                    }
                }
            }

            return updates.OrderBy(x => x.Changed.Skey).ToList();
        }

        public void ApplyDbUpdateResult(List<DbUpdate<ChatSession>> updates)
        {
            m_log.Debug("ApplyDbUpdateResult()");

            lock (m_lock)
            {
                foreach (var u in updates.Where(x => x.Success))
                {
                    var entry = LookupBySkey(u.Changed.Skey);
                    if (entry == null) continue;
                    lock (entry)
                        entry.ApplyDbUpdateResult(u);
                }
            }
        }

        public void UpdateDb(IChatDatabaseFactory dbFactory, List<DbUpdate<ChatSession>> updates)
        {
            var byUpdateType = updates.ToLookup(x => x.Original == null);

            m_log.DebugFormat(
                "UpdateDb: Going to create {0} new objects and update {1} objects",
                byUpdateType[true].Count(),
                byUpdateType[false].Count());

            try
            {
                dbFactory.Query(
                    db =>
                        {
                            foreach (var u in byUpdateType[true])
                            {
                                try
                                {
                                    ChatSession.Insert(db, u.Changed);
                                }
                                catch (Exception e)
                                {
                                    m_log.Error($"Create {u.Changed.Skey} failed.", e);
                                    u.SetFailed();
                                }
                            }

                            foreach (var u in byUpdateType[false])
                            {
                                try
                                {
                                    ChatSession.Update(db, u.Original, u.Changed);
                                }
                                catch (Exception e)
                                {
                                    m_log.Error($"Update {u.Changed.Skey} failed.", e);
                                    u.SetFailed();
                                }
                            }
                        });
            }
            catch (Exception e)
            {
                m_log.Error("Create/update objects batch failed", e);
                foreach (var x in updates) x.SetFailed();
            }

            m_log.DebugFormat(
                "UpdateDb: create objects {0} succeeded, {1} failed; update objects {2} succeeded, {3} failed",
                byUpdateType[true].Count(x => x.Success),
                byUpdateType[true].Count(x => !x.Success),
                byUpdateType[false].Count(x => x.Success),
                byUpdateType[false].Count(x => !x.Success));
        }

        #endregion

        #region Id generation

        private long GetNewSessionSkey()
        {
            return Interlocked.Increment(ref m_lastSessionSkey);
        }

        private long GetNewSessionEventSkey()
        {
            return Interlocked.Increment(ref m_lastEventSkey);
        }

        #endregion


        #region Session History

        private List<ChatSession> GetHistorySessions(
            uint customerId,
            decimal userId,
            HashSet<uint> visibleDepartments)
        {
            List<StorageEntry> entries;
            lock (m_lock)
            {
                entries = m_list
                    .Where(x => x.Object().CustomerId == customerId)
                    .Where(
                        x =>
                            x.Object().AgentsInvolved.Contains(userId)
                            || x.Object().DepartmentsInvolved.Overlaps(visibleDepartments))
                    .ToList();
            }

            return entries
                .Select(
                    x =>
                        {
                            lock (x) return x.ObjectCopy();
                        })
                .ToList();
        }

        public List<ChatSession> Search(
            uint customerId,
            decimal userId,
            HashSet<uint> visibleDepartments,
            SessionSearchFilter filter,
            int pageSize,
            int pageNumber)
        {
            var query = GetHistorySessions(customerId, userId, visibleDepartments).AsQueryable();

            if (filter.Agents != null && filter.Agents.Count > 0)
            {
                query = query.Where(x => x.AgentsInvolved.Overlaps(filter.Agents));
            }

            query = query.Where(x => x.Messages.Any(y => filter.StartDate <= y.TimestampUtc.Date && y.TimestampUtc.Date <= filter.EndDate));

            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                query = query.Where(x => x.Messages.Any(m => m.Text.ToLower().Contains(filter.SearchString.ToLower())));
            }

            query = query.OrderByDescending(x => x.Skey);
            query = query.Skip(pageSize * (pageNumber - 1)).Take(pageSize + 1);

            return query.ToList();
        }

        public List<ChatSessionMessage> GetMessages(
            uint customerId,
            long sessionSkey,
            int pageSize,
            int pageNumber)
        {
            var session = Get(customerId, sessionSkey);
            return session?.Messages
                       .OrderBy(x => x.Id)
                       .Skip(pageSize * (pageNumber - 1))
                       .Take(pageSize + 1).ToList()
                   ?? new List<ChatSessionMessage>();
        }

        #endregion
    }
}