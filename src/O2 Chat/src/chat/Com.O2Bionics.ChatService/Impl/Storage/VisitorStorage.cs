using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.Utils;
using log4net;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public class VisitorUpdate
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public MediaSupport? MediaSupport { get; set; }
        public VisitorSendTranscriptMode? TranscriptMode { get; set; }
    }

    public class VisitorStorage : IVisitorStorage
    {
        private class StorageEntry
        {
            public StorageEntry(Visitor original, Visitor changed = null)
            {
                var isNewObject = ReferenceEquals(original, null);
                if (isNewObject && ReferenceEquals(changed, null))
                    throw new InvalidOperationException("original and update can't be null simultaneously");
                m_original = original;
                m_changed = changed;
                IsDirty = isNewObject;
            }

            public bool IsDirty { get; private set; }

            private Visitor m_original;
            private Visitor m_changed;

            public ulong Id => (m_changed ?? m_original).Id;

            public Visitor ObjectCopy()
            {
                return new Visitor(m_changed ?? m_original);
            }

            public VisitorUpdate Update(DateTime utcNow, VisitorUpdate update)
            {
                var obj = m_changed ?? new Visitor(m_original);

                var actualUpdate = new VisitorUpdate();
                var changed = false;

                if (update.Name != null && obj.Name != update.Name)
                {
                    actualUpdate.Name = obj.Name = update.Name;
                    changed = true;
                }

                if (update.Email != null && obj.Email != update.Email)
                {
                    actualUpdate.Email = obj.Email = update.Email;
                    changed = true;
                }

                if (update.Phone != null && obj.Phone != update.Phone)
                {
                    actualUpdate.Phone = obj.Phone = update.Phone;
                    changed = true;
                }

                if (update.MediaSupport != null && obj.MediaSupport != update.MediaSupport.Value)
                {
                    actualUpdate.MediaSupport = obj.MediaSupport = update.MediaSupport;
                    changed = true;
                }

                if (update.TranscriptMode != null && obj.TranscriptMode != update.TranscriptMode.Value)
                {
                    actualUpdate.TranscriptMode = obj.TranscriptMode = update.TranscriptMode.Value;
                    changed = true;
                }

                if (changed)
                    obj.UpdateTimestampUtc = utcNow;

                if (changed)
                {
                    m_changed = obj;
                    IsDirty = true;
                }

                return actualUpdate;
            }

            public void ApplyDbUpdateResult(DbUpdate<Visitor> dbUpdate)
            {
                m_original = dbUpdate.Changed;

                IsDirty =
                    m_original.UpdateTimestampUtc != m_changed.UpdateTimestampUtc
                    || m_original.Name != m_changed.Name
                    || m_original.Email != m_changed.Email
                    || m_original.Phone != m_changed.Phone
                    || m_original.MediaSupport != m_changed.MediaSupport
                    || m_original.TranscriptMode != m_changed.TranscriptMode;
                if (!IsDirty) m_changed = null;
            }

            public DbUpdate<Visitor> GetDbUpdate()
            {
                return new DbUpdate<Visitor>(m_original, new Visitor(m_changed));
            }
        }


        private static readonly ILog m_log = LogManager.GetLogger(typeof(VisitorStorage));

        private readonly object m_lock = new object();

        private readonly LinkedList<StorageEntry> m_list = new LinkedList<StorageEntry>();
        private readonly int m_capacity;

        private readonly Dictionary<ulong, LinkedListNode<StorageEntry>> m_visitorById =
            new Dictionary<ulong, LinkedListNode<StorageEntry>>();


        private readonly INowProvider m_nowProvider;
        private readonly IChatDatabaseFactory m_databaseFactory;

        public VisitorStorage(
            INowProvider nowProvider,
            IChatDatabaseFactory databaseFactory,
            ChatServiceSettings serviceSettings)
        {
            m_nowProvider = nowProvider;
            m_databaseFactory = databaseFactory;
            m_capacity = serviceSettings.Cache.Visitor;

            m_log.DebugFormat("created with size={0}", m_capacity);
        }

        public Visitor Get(ulong id)
        {
            m_log.DebugFormat("get {0}", id);

            lock (m_lock)
            {
                var entry = LookupById(id);
                if (entry != null) return entry.ObjectCopy();
            }

            return CreateBo(LookupDbById(id));
        }

        public Visitor GetOrCreate(uint customerId, ulong id)
        {
            m_log.DebugFormat("get or create {0} {1}", id, customerId);

            lock (m_lock)
            {
                var entry = LookupById(id);
                if (entry != null) return entry.ObjectCopy();
            }

            return CreateBo(LookupDbById(id)) ?? CreateNew(customerId, id);
        }

        public VisitorUpdate Update(ulong id, VisitorUpdate update)
        {
            if (update == null) throw new ArgumentNullException("update");

            m_log.DebugFormat("update {0} {1}", id, update.JsonStringify());

            var utcNow = m_nowProvider.UtcNow;

            lock (m_lock)
            {
                var entry1 = LookupById(id);
                if (entry1 != null) return entry1.Update(utcNow, update);
            }

            var record = LookupDbById(id);
            if (record == null) return null;

            var entry2 = new StorageEntry(new Visitor(record));
            lock (m_lock)
            {
                var entry3 = LookupById(id);
                if (entry3 != null) return entry3.Update(utcNow, update);

                AddHead(entry2);
                return entry2.Update(utcNow, update);
            }
        }

        private Visitor CreateNew(uint customerId, ulong id)
        {
            var now = m_nowProvider.UtcNow;
            var visitor = new Visitor(customerId, id, now);
            var entry1 = new StorageEntry(null, visitor);

            lock (m_lock)
            {
                var entry2 = LookupById(id);
                if (entry2 != null) return entry2.ObjectCopy();

                AddHead(entry1);
                return entry1.ObjectCopy();
            }
        }

        private Visitor CreateBo(VISITOR x)
        {
            if (x == null) return null;

            var visitor = new Visitor(x);
            var entry1 = new StorageEntry(visitor);

            lock (m_lock)
            {
                var entry2 = LookupById(visitor.Id);
                if (entry2 != null) return entry2.ObjectCopy();

                AddHead(entry1);
                return entry1.ObjectCopy();
            }
        }

        private VISITOR LookupDbById(ulong id)
        {
            return m_databaseFactory.Query(
                db => db.VISITORs.FirstOrDefault(x => x.VISITOR_ID == id));
        }

        #region List management. All operations should be locked.

        private StorageEntry LookupById(ulong id)
        {
            LinkedListNode<StorageEntry> node;
            return m_visitorById.TryGetValue(id, out node) ? MakeFirst(node) : null;
        }

        private void AddHead(StorageEntry entry)
        {
            RemoveTail();

            var node = m_list.AddFirst(entry);
            m_visitorById[entry.Id] = node;
        }

        private void RemoveTail()
        {
            var n = m_list.Last;
            while (n != null && m_list.Count >= m_capacity)
            {
                if (n.Value.IsDirty)
                {
                    n = n.Previous;
                    continue;
                }

                var prev = n.Previous;
                RemoveNode(n);
                n = prev;
            }
        }

        private void RemoveNode(LinkedListNode<StorageEntry> node)
        {
            m_visitorById.Remove(node.Value.Id);
            m_list.Remove(node);
        }

        private StorageEntry MakeFirst(LinkedListNode<StorageEntry> node)
        {
            m_list.Remove(node);
            m_list.AddFirst(node);
            return node.Value;
        }

        #endregion

        #region IDbUpdaterStorage<>

        public void Load(ChatDatabase db)
        {
        }

        public List<DbUpdate<Visitor>> GetDbUpdates()
        {
            lock (m_lock)
            {
                return m_list
                    .Where(x => x.IsDirty)
                    .Select(x => x.GetDbUpdate())
                    .ToList();
            }
        }

        public void ApplyDbUpdateResult(List<DbUpdate<Visitor>> updates)
        {
            m_log.Debug("ApplyDbUpdateResult()");

            lock (m_lock)
            {
                // nodes can't be removed while dirty
                foreach (var u in updates.Where(x => x.Success))
                {
                    var entry = LookupById(u.Changed.Id);
                    entry?.ApplyDbUpdateResult(u);
                }
            }
        }

        public void UpdateDb(IChatDatabaseFactory dbFactory, List<DbUpdate<Visitor>> updates)
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
                                    Visitor.Insert(db, u.Changed);
                                }
                                catch (Exception e)
                                {
                                    m_log.Error($"Create {u.Changed.Id} failed.", e);
                                    u.SetFailed();
                                }
                            }

                            foreach (var u in byUpdateType[false])
                            {
                                try
                                {
                                    Visitor.Update(db, u.Original, u.Changed);
                                }
                                catch (Exception e)
                                {
                                    m_log.Error($"Update {u.Changed.Id} failed.", e);
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