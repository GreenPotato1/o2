using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.Utils;
using log4net;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public class CannedMessageStorage : ICannedMessageStorage
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(UserStorage));

        private readonly ConcurrentDictionary<decimal, List<CannedMessage>> m_customerCannedMessage
            = new ConcurrentDictionary<decimal, List<CannedMessage>>();

        private readonly INowProvider m_nowProvider;

        public CannedMessageStorage(INowProvider nowProvider)
        {
            m_nowProvider = nowProvider;
        }

        public CannedMessage Get(ChatDatabase db, uint customerId, uint id)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            List<CannedMessage> list;
            if (!m_customerCannedMessage.TryGetValue(customerId, out list))
            {
                list = LoadAndCache(db, customerId);
            }

            return list.FirstOrDefault(x => x.Id == id);
        }

        public List<CannedMessage> GetMany(ChatDatabase db, uint customerId, decimal? userId, HashSet<uint> departments)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            List<CannedMessage> list;
            if (!m_customerCannedMessage.TryGetValue(customerId, out list))
            {
                list = LoadAndCache(db, customerId);
            }

            return Filter(list, userId, departments);
        }

        private static List<CannedMessage> Filter(IReadOnlyCollection<CannedMessage> list, decimal? userId, HashSet<uint> departments)
        {
            var result = Enumerable.Empty<CannedMessage>();

            if (userId.HasValue)
                result = result.Concat(list.Where(x => x.UserId == userId));
            if (departments != null && departments.Any())
                result = result.Concat(list.Where(x => x.DepartmentId.HasValue && departments.Contains(x.DepartmentId.Value)));

            return result.ToList();
        }

        private List<CannedMessage> LoadAndCache(ChatDatabase db, uint customerId)
        {
            var list = CannedMessage.GetCustomerData(db, customerId);
            if (list.Count == 0)
            {
                List<CannedMessage> _;
                m_customerCannedMessage.TryRemove(customerId, out _);
            }
            else
            {
                m_customerCannedMessage[customerId] = list;
            }

            return list;
        }

        public CannedMessage CreateNew(ChatDatabase db, uint customerId, CannedMessage cannedMessage)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (cannedMessage == null) throw new ArgumentNullException(nameof(cannedMessage));

            var messages = new CannedMessage.Validator().ValidateNew(cannedMessage);
            if (messages.Count > 0)
                throw new ValidationException(messages);

            var created = CannedMessage.Insert(db, m_nowProvider.UtcNow, customerId, cannedMessage);
            ScheduleCacheResetOnCommit(db, customerId);

            return created;
        }

        public CannedMessage Update(ChatDatabase db, uint customerId, uint id, CannedMessage.UpdateInfo update)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (update == null) throw new ArgumentNullException(nameof(update));

            var messages = new CannedMessage.Validator().ValidateUpdate(update);
            if (messages.Count > 0)
                throw new ValidationException(messages);

            m_log.DebugFormat("updating user with id={0}", id);

            var updated = CannedMessage.Update(db, m_nowProvider.UtcNow, id, update);
            ScheduleCacheResetOnCommit(db, customerId);

            return updated;
        }

        public void Delete(ChatDatabase db, uint customerId, uint id)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            CannedMessage.Delete(db, id);
            ScheduleCacheResetOnCommit(db, customerId);
        }

        private void ScheduleCacheResetOnCommit(ChatDatabase db, uint customerId)
        {
            db.OnCommitActions.Add(
                () =>
                    {
                        List<CannedMessage> _;
                        m_customerCannedMessage.TryRemove(customerId, out _);
                    });
        }
    }
}