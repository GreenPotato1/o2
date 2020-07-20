using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.Utils;
using log4net;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public class AgentSessionUpdate
    {
        public DateTime? LastAccessTimestampUtc { get; set; }
    }

    public class AgentSessionStorage : IAgentSessionStorage
    {
        private class StorageEntry
        {
            public StorageEntry(AgentSession original, AgentSession changed = null)
            {
                var isNewObject = ReferenceEquals(original, null);
                if (isNewObject && ReferenceEquals(changed, null))
                    throw new InvalidOperationException("original and update can't be null simultaneously");
                m_original = original;
                m_changed = changed;
                IsDirty = isNewObject;
            }

            public bool IsDirty { get; private set; }

            private AgentSession m_original;
            private AgentSession m_changed;

            private readonly HashSet<string> m_connections = new HashSet<string>();

            public bool IsConnected => m_connections.Count > 0;

            public void AddConnection(string connectionId)
            {
                m_connections.Add(connectionId);
            }

            public void AddConnections(IEnumerable<string> connectionIds)
            {
                m_connections.UnionWith(connectionIds);
            }

            public void RemoveConnection(string connectionId)
            {
                m_connections.Remove(connectionId);
            }

            public void RemoveAllConnections()
            {
                m_connections.Clear();
            }

            public AgentSession Object()
            {
                return m_changed ?? m_original;
            }

            public Guid Guid => Object().Guid;

            public uint CustomerId => Object().CustomerId;

            public AgentSession ObjectCopy()
            {
                return new AgentSession(Object());
            }

            public void Update(AgentSessionUpdate update)
            {
                var obj = m_changed ?? new AgentSession(m_original);

                var actualUpdate = new AgentSessionUpdate();
                var changed = false;

                if (update.LastAccessTimestampUtc.HasValue && obj.LastAccessTimestampUtc != update.LastAccessTimestampUtc.Value)
                {
                    actualUpdate.LastAccessTimestampUtc = obj.LastAccessTimestampUtc = update.LastAccessTimestampUtc.Value;
                    changed = true;
                }

                if (changed)
                {
                    m_changed = obj;
                    IsDirty = true;
                }
            }

            public void ApplyDbUpdateResult(DbUpdate<AgentSession> dbUpdate)
            {
                m_original = dbUpdate.Changed;

                IsDirty =
                    m_original.LastAccessTimestampUtc != m_changed.LastAccessTimestampUtc;
                if (!IsDirty) m_changed = null;
            }

            public DbUpdate<AgentSession> GetDbUpdate()
            {
                return new DbUpdate<AgentSession>(m_original, new AgentSession(m_changed));
            }
        }


        private static readonly ILog m_log = LogManager.GetLogger(typeof(AgentSessionStorage));

        private readonly object m_lock = new object();

        private readonly LinkedList<StorageEntry> m_list = new LinkedList<StorageEntry>();

        private readonly Dictionary<Guid, LinkedListNode<StorageEntry>> m_entryByGuid =
            new Dictionary<Guid, LinkedListNode<StorageEntry>>();

        private readonly Dictionary<decimal, List<LinkedListNode<StorageEntry>>> m_entryByCustomerId =
            new Dictionary<decimal, List<LinkedListNode<StorageEntry>>>();


        private readonly INowProvider m_nowProvider;

        public AgentSessionStorage(
            INowProvider nowProvider)
        {
            m_nowProvider = nowProvider;
        }

        public AgentSession Get(Guid guid)
        {
            StorageEntry entry;
            lock (m_lock)
                entry = LookupByGuid(guid);

            if (entry == null) return null;

            var utcNow = m_nowProvider.UtcNow;
            lock (entry)
            {
                entry.Update(new AgentSessionUpdate { LastAccessTimestampUtc = utcNow });
                return entry.ObjectCopy();
            }
        }

        public AgentSession GetOrCreate(Guid guid, uint customerId, uint userId)
        {
            StorageEntry entry;
            lock (m_lock)
                entry = LookupByGuid(guid);

            if (entry == null)
            {
                m_log.DebugFormat("creating new agent session for agent {0}", userId);

                var now = m_nowProvider.UtcNow;
                var session = new AgentSession(guid, customerId, userId, now);
                entry = new StorageEntry(null, session);

                lock (m_lock)
                {
                    var entry2 = LookupByGuid(guid);
                    if (entry2 == null)
                        AddEntry(entry);
                    entry = entry2 ?? entry;
                }
            }

            var utcNow = m_nowProvider.UtcNow;
            lock (entry)
            {
                entry.Update(new AgentSessionUpdate { LastAccessTimestampUtc = utcNow });
                return entry.ObjectCopy();
            }
        }

        public HashSet<uint> GetConnectedUsers(uint customerId)
        {
            List<StorageEntry> entries;
            lock (m_lock)
                entries = LookupByCustomer(customerId);

            var result = new HashSet<uint>();
            foreach (var entry in entries)
            {
                lock (entry)
                {
                    if (entry.IsConnected)
                        result.Add(entry.Object().AgentId);
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="connectionId"></param>
        /// <returns>returns online agents</returns>
        public HashSet<uint> AddConnection(Guid guid, string connectionId)
        {
            StorageEntry entry;
            lock (m_lock)
                entry = LookupByGuid(guid);

            if (entry == null) throw new InvalidOperationException("Session not found guid=" + guid);

            lock (entry)
                entry.AddConnection(connectionId);

            return GetConnectedUsers(entry.CustomerId);
        }

        public HashSet<uint> RemoveConnection(Guid guid, string connectionId)
        {
            StorageEntry entry;
            lock (m_lock)
                entry = LookupByGuid(guid);

            if (entry == null) throw new InvalidOperationException("Session not found guid=" + guid);

            lock (entry)
                entry.RemoveConnection(connectionId);

            return GetConnectedUsers(entry.CustomerId);
        }

        public void DisconnectAll()
        {
            lock (m_lock)
                foreach (var entry in m_list)
                    lock (entry)
                        entry.RemoveAllConnections();
        }

        public void AddConnectedSessions(List<KeyValuePair<string, Guid>> list)
        {
            var sessionToConnection = list.ToLookup(x => x.Value, x => x.Key);
            lock (m_list)
            {
                foreach (var sc in sessionToConnection.OrderBy(x => x.Key))
                {
                    var entry = LookupByGuid(sc.Key);
                    if (entry != null)
                        lock (entry)
                            entry.AddConnections(sc);
                }
            }
        }

        #region List management. All operations should be locked.

        private StorageEntry LookupByGuid(Guid guid)
        {
            LinkedListNode<StorageEntry> node;
            return m_entryByGuid.TryGetValue(guid, out node) ? MakeFirst(node) : null;
        }

        private List<StorageEntry> LookupByCustomer(uint customerId)
        {
            List<LinkedListNode<StorageEntry>> list;
            if (!m_entryByCustomerId.TryGetValue(customerId, out list))
                return new List<StorageEntry>();
            return list.Select(x => x.Value).OrderBy(x => x.Guid).ToList();
        }

        private StorageEntry MakeFirst(LinkedListNode<StorageEntry> node)
        {
            m_list.Remove(node);
            m_list.AddFirst(node);
            return node.Value;
        }

        private void AddEntry(StorageEntry entry)
        {
            m_log.DebugFormat("add {0}", entry.Guid);

            var node = m_list.AddFirst(entry);

            var obj = entry.Object();
            m_entryByGuid[obj.Guid] = node;

            List<LinkedListNode<StorageEntry>> entryList;
            if (m_entryByCustomerId.TryGetValue(entry.CustomerId, out entryList))
                entryList.Add(node);
            else
                m_entryByCustomerId.Add(entry.CustomerId, new List<LinkedListNode<StorageEntry>> { node });
        }

        private void RemoveNode(LinkedListNode<StorageEntry> node)
        {
            List<LinkedListNode<StorageEntry>> entryList;
            if (m_entryByCustomerId.TryGetValue(node.Value.CustomerId, out entryList))
                entryList.Remove(node);

            var obj = node.Value.Object();
            m_entryByGuid.Remove(obj.Guid);

            m_list.Remove(node);
        }

        #endregion

        #region IDbUpdaterStorage<>

        public void Load(ChatDatabase db)
        {
            var sessions = db.AGENT_SESSION.Select(x => new AgentSession(x)).ToList();
            foreach (var session in sessions)
                AddEntry(new StorageEntry(session));
        }

        public List<DbUpdate<AgentSession>> GetDbUpdates()
        {
            var updates = new List<DbUpdate<AgentSession>>();
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

            return updates.OrderBy(x => x.Changed.Guid).ToList();
        }

        public void ApplyDbUpdateResult(List<DbUpdate<AgentSession>> updates)
        {
            m_log.Debug("ApplyDbUpdateResult()");

            lock (m_lock)
            {
                foreach (var u in updates.Where(x => x.Success))
                {
                    var entry = LookupByGuid(u.Changed.Guid);
                    if (entry == null) continue;
                    lock (entry)
                        entry.ApplyDbUpdateResult(u);
                }
            }
        }

        public void UpdateDb(IChatDatabaseFactory dbFactory, List<DbUpdate<AgentSession>> updates)
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
                                    AgentSession.Insert(db, u.Changed);
                                }
                                catch (Exception e)
                                {
                                    m_log.Error($"Create {u.Changed.Guid} failed.", e);
                                    u.SetFailed();
                                }
                            }

                            foreach (var u in byUpdateType[false])
                            {
                                try
                                {
                                    AgentSession.Update(db, u.Original, u.Changed);
                                }
                                catch (Exception e)
                                {
                                    m_log.Error($"Update {u.Changed.Guid} failed.", e);
                                    u.SetFailed();
                                }
                            }
                        });
            }
            catch (Exception e)
            {
                m_log.Error("Create/update objects batch failed.", e);
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
    }
}