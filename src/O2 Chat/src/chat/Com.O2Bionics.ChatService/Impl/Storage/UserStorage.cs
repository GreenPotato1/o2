using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.ChatService.Properties;
using Com.O2Bionics.Utils;
using log4net;
using LinqToDB;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public class UserStorage : IUserStorage
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(UserStorage));

        private readonly ConcurrentDictionary<uint, Dictionary<uint, User>> m_customerUsers
            = new ConcurrentDictionary<uint, Dictionary<uint, User>>();

        private readonly INowProvider m_nowProvider;

        public UserStorage(
            INowProvider nowProvider)
        {
            m_nowProvider = nowProvider;
        }

        public User Get(ChatDatabase db, uint customerId, uint id)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            Dictionary<uint, User> users;
            if (!m_customerUsers.TryGetValue(customerId, out users))
            {
                users = LoadAndCache(db, customerId);
            }

            User user;
            return users.TryGetValue(id, out user) ? user : null;
        }

        public User GetByEmail(ChatDatabase db, string email, bool skipDisabled)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(email)) return null;

            var key = User.GetKeyByEmail(db, email, skipDisabled);
            return key == null ? null : Get(db, key.Item1.Value, key.Item2);
        }

        public List<User> GetAll(ChatDatabase db, uint customerId)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            Dictionary<uint, User> users;
            if (!m_customerUsers.TryGetValue(customerId, out users))
            {
                users = LoadAndCache(db, customerId);
            }

            return users.Values.ToList();
        }

        public HashSet<uint> GetOnline(ChatDatabase db, uint customerId)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));

            Dictionary<uint, User> users;
            if (!m_customerUsers.TryGetValue(customerId, out users))
            {
                users = LoadAndCache(db, customerId);
            }

            return new HashSet<uint>(users.Values.Where(x => x.IsOnline).Select(x => x.Id));
        }

        private Dictionary<uint, User> LoadAndCache(ChatDatabase db, uint customerId)
        {
            var users = User.GetAll(db, customerId);
            if (users.Count == 0)
            {
                Dictionary<uint, User> t;
                m_customerUsers.TryRemove(customerId, out t);
            }
            else
            {
                m_customerUsers[customerId] = users;
            }

            return users;
        }

        public HashSet<uint> GetCustomerAgentsDepartments(ChatDatabase db, uint customerId, HashSet<uint> agentIds)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (agentIds == null) throw new ArgumentNullException(nameof(agentIds));

            if (agentIds.Count == 0) return new HashSet<uint>();

            var users = GetAll(db, customerId);
            var departments = new HashSet<uint>();
            foreach (var entry in users)
            {
                if (agentIds.Contains(entry.Id))
                    departments.UnionWith(entry.AgentDepartmentIds);
            }

            return departments;
        }

        public User CreateNew(ChatDatabase db, User user)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (user == null) throw new ArgumentNullException(nameof(user));

            var messages = new User.Validator().ValidateNew(user);
            if (messages.Any()) throw new ValidationException(messages);

            var sameEmailUser = GetByEmail(db, user.Email, false);
            if (sameEmailUser != null)
            {
                messages.Add(new ValidationMessage("email", "User already exists with provided email"));
                throw new ValidationException(messages);
            }

            m_log.DebugFormat("creating new user with customer={0}, email={1}", user.CustomerId, user.Email);

            var created = User.Insert(db, m_nowProvider.UtcNow, user);

            {
                var customerId = user.CustomerId;
                db.OnCommitActions.Add(
                    () =>
                        {
                            Dictionary<uint, User> t;
                            m_customerUsers.TryRemove(customerId, out t);
                        });
            }

            return created;
        }

        // caller must reset all customer' user caches after transaction end
        public User Update(ChatDatabase db, uint customerId, uint id, User.UpdateInfo update, bool resetCache = false)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (update == null) throw new ArgumentNullException(nameof(update));

            var messages = new User.Validator().ValidateUpdate(update);
            if (messages.Count > 0)
                throw new ValidationException(messages);

            if (update.Email != null)
            {
                var sameEmailUser = GetByEmail(db, update.Email, false);
                if (sameEmailUser != null && sameEmailUser.Id != id)
                {
                    messages.Add(new ValidationMessage("email", "Other user already exists with provided email"));
                    throw new ValidationException(messages);
                }
            }

            var user = Get(db, customerId, id);
            if (user == null)
                throw new InvalidOperationException(string.Format(Resources.UserNotFoundError1, id));

            m_log.DebugFormat("updating user with id={0}, email={1}", id, user.Email);

            var updated = User.Update(db, m_nowProvider.UtcNow, id, update);

            if (resetCache)
            {
                Dictionary<uint, User> t;
                m_customerUsers.TryRemove(customerId, out t);
            }
            else
            {
                db.OnCommitActions.Add(
                    () =>
                        {
                            Dictionary<uint, User> t;
                            m_customerUsers.TryRemove(customerId, out t);
                        });
            }

            return updated;
        }

        #region Login related

        public void RecordResetPasswordCode(ChatDatabase db, uint userId, string email, string code)
        {
            var codeHash = code.ToPasswordHash();
            m_log.DebugFormat("RecordResetPasswordCode for userId={0}, code={1}, codeHash={2}", userId, code, codeHash);
            var now = m_nowProvider.UtcNow;
            var record = new FORGOT_PASSWORD
                {
                    USER_ID = userId,
                    CREATE_TIMESTAMP = now,
                    EMAIL = email,
                    CODE = codeHash
                };
            db.InsertWithIdentity(record);
        }

        public void DeleteResetPasswordCode(ChatDatabase db, string code)
        {
            var codeHash = code.ToPasswordHash();
            m_log.DebugFormat("DeleteResetPasswordCode for code={0}, codeHash={1}", code, codeHash);
            db.FORGOT_PASSWORD
                .Where(x => x.CODE == codeHash)
                .Delete();
        }

        /// <returns> Tuple (customerId, userId)</returns>
        public Tuple<uint?, uint?> GetResetPasswordCodeUserId(ChatDatabase db, DateTime minTimestampUtc, string code)
        {
            var codeHash = code.ToPasswordHash();
            m_log.DebugFormat("ResetPassword for code={0}, codeHash={1} minTimestamp={2}", code, codeHash, minTimestampUtc);
            var minTimestampLocal = minTimestampUtc;
            var user =
                (from x in db.FORGOT_PASSWORD
                    join u in db.CUSTOMER_USER on x.USER_ID equals u.ID
                    where x.CODE == codeHash && x.CREATE_TIMESTAMP >= minTimestampLocal
                    orderby x.CREATE_TIMESTAMP descending
                    select new { u.CUSTOMER_ID, x.USER_ID })
                .FirstOrDefault();
            return user != null ? Tuple.Create(user.CUSTOMER_ID, user.USER_ID) : null;
        }

        #endregion
    }
}