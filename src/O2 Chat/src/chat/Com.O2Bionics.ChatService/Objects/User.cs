using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.Utils;
using LinqToDB;
using LinqToDB.Data;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.ChatService.Objects
{
    public class User : IHasCompositeFullName
    {
        private static readonly string m_tableName = typeof(CUSTOMER_USER).Name;

        public uint Id { get; }
        public uint CustomerId { get; }

        public DateTime AddTimestampUtc { get; }
        public DateTime UpdateTimestampUtc { get; }

        public bool IsOnline { get; }

        public ObjectStatus Status { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string PasswordHash { get; }

        public bool IsOwner { get; }
        public bool IsAdmin { get; }
        public HashSet<uint> AgentDepartmentIds { get; }
        public HashSet<uint> SupervisorDepartmentIds { get; }

        public string Avatar { get; }

        public User(
            uint customerId,
            ObjectStatus status,
            string email,
            string firstName,
            string lastName,
            string passwordHash,
            string avatar,
            bool isOwner,
            bool isAdmin,
            IEnumerable<uint> agentDepartments,
            IEnumerable<uint> supervisorDepartments)
        {
            CustomerId = customerId;
            AddTimestampUtc = DateTime.MinValue;

            UpdateTimestampUtc = DateTime.MinValue;

            IsOnline = true;

            Status = status;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            PasswordHash = passwordHash;
            Avatar = avatar;

            IsOwner = isOwner;
            IsAdmin = isAdmin;
            AgentDepartmentIds = agentDepartments.ToHashSet();
            SupervisorDepartmentIds = supervisorDepartments.ToHashSet();
        }

        private class UserRoleData
        {
            public int RoleId { get; set; }
            public uint? DepartmentId { get; set; }
        }

        private User(CUSTOMER_USER dbo, IEnumerable<UserRoleData> roleData)
        {
            Id = dbo.ID;
            CustomerId = dbo.CUSTOMER_ID.Value;
            Status = (ObjectStatus)dbo.STATUS_ID;

            AddTimestampUtc = dbo.CREATE_TIMESTAMP;
            UpdateTimestampUtc = dbo.UPDATE_TIMESTAMP;

            IsOnline = dbo.IS_ONLINE == 1;

            Email = dbo.EMAIL;
            FirstName = dbo.FIRST_NAME;
            LastName = dbo.LAST_NAME ?? "";
            PasswordHash = dbo.PASSWORD;
            Avatar = dbo.AVATAR;

            var rolesByType = roleData.ToLookup(x => x.RoleId, x => x.DepartmentId);
            IsOwner = rolesByType[(int)UserRoleCode.Owner].Any();
            IsAdmin = rolesByType[(int)UserRoleCode.Admin].Any();
            AgentDepartmentIds = rolesByType[(int)UserRoleCode.Agent]
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToHashSet();
            SupervisorDepartmentIds = rolesByType[(int)UserRoleCode.Supervisor]
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToHashSet();
        }

        public AgentInfo AsInfo()
        {
            return new AgentInfo
                {
                    Id = Id,
                    CustomerId = CustomerId,
                    Email = Email,
                    FirstName = FirstName,
                    LastName = LastName,
                    Avatar = Avatar,
                };
        }

        public UserInfo AsUserInfo()
        {
            return new UserInfo
                {
                    Id = Id,
                    CustomerId = CustomerId,
                    AddTimestampUtc = AddTimestampUtc,
                    UpdateTimestampUtc = UpdateTimestampUtc,
                    Status = Status,
                    Email = Email,
                    FirstName = FirstName,
                    LastName = LastName,
                    Avatar = Avatar,
                    IsOwner = IsOwner,
                    IsAdmin = IsAdmin,
                    AgentDepartments = new HashSet<uint>(AgentDepartmentIds),
                    SupervisorDepartments = new HashSet<uint>(SupervisorDepartmentIds),
                };
        }

        public bool IsInRole(UserRole role)
        {
            switch (role.Role)
            {
                case UserRoleCode.Agent:
                    return AgentDepartmentIds.Contains(role.DepartmentId);
                case UserRoleCode.Supervisor:
                    return SupervisorDepartmentIds.Contains(role.DepartmentId);
                case UserRoleCode.Admin:
                    return IsAdmin;
                case UserRoleCode.Owner:
                    return IsOwner;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role.Role, "Unsupported role code");
            }
        }

        public bool IsInRole(UserRoleCode role)
        {
            switch (role)
            {
                case UserRoleCode.Agent:
                    return AgentDepartmentIds.Any();
                case UserRoleCode.Supervisor:
                    return SupervisorDepartmentIds.Any();
                case UserRoleCode.Admin:
                    return IsAdmin;
                case UserRoleCode.Owner:
                    return IsOwner;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, "Unsupported role code");
            }
        }

        public static Tuple<uint?, uint> GetKeyByEmail(ChatDatabase db, string email, bool skipDisabled)
        {
            var query = db.CUSTOMER_USER.Where(x => x.EMAIL == email && x.STATUS_ID != (int)ObjectStatus.Deleted);
            if (skipDisabled)
                query = query.Where(x => x.STATUS_ID != (int)ObjectStatus.Disabled);

            var key = query.Select(x => new { x.CUSTOMER_ID, x.ID }).FirstOrDefault();
            return key == null ? null : Tuple.Create(key.CUSTOMER_ID, key.ID);
        }

        public static Dictionary<uint, User> GetAll(ChatDatabase db, uint customerId)
        {
            var users = db.CUSTOMER_USER
                .Where(x => x.CUSTOMER_ID == customerId && x.STATUS_ID != (int)ObjectStatus.Deleted)
                .ToList();
            var userIds = users.Select(x => x.ID).ToList();
            var roles = db.USER_ROLE
                .Where(x => userIds.Contains(x.USER_ID.Value))
                .ToLookup(
                    x => x.USER_ID,
                    x => new UserRoleData { RoleId = (int)x.ROLE_ID, DepartmentId = x.DEPARTMENT_ID });
            return users
                .Select(x => new User(x, roles[x.ID]))
                .ToDictionary(x => x.Id, x => x);
        }

        private static User Get(ChatDatabase db, uint id)
        {
            var user = db.CUSTOMER_USER
                .FirstOrDefault(x => x.ID == id && x.STATUS_ID != (int)ObjectStatus.Deleted);
            if (user == null) return null;
            var roles = db.USER_ROLE
                .Where(x => x.USER_ID == id)
                .Select(x => new UserRoleData { RoleId = (int)x.ROLE_ID, DepartmentId = x.DEPARTMENT_ID });
            return new User(user, roles);
        }

        public static User Insert(ChatDatabase db, DateTime utcNow, User obj)
        {
            // manual query generation is used here because linq2db built queries use varchar parameter type
            // for nvarchar fields. this leads to loose of non-ascii characters.

            var newId = EntityCreateUtility.GenerateId();
            var pp = new List<DataParameter>
                {
                    new DataParameter("ID", newId),
                    new DataParameter("CUSTOMER_ID", obj.CustomerId),
                    new DataParameter("CREATE_TIMESTAMP", utcNow),
                    new DataParameter("UPDATE_TIMESTAMP", utcNow),
                    new DataParameter("IS_ONLINE", (sbyte)(obj.IsOnline ? 1 : 0)),
                    new DataParameter("STATUS_ID", (int)obj.Status),
                    new DataParameter("EMAIL", obj.Email, DataType.NText),
                    new DataParameter("FIRST_NAME", obj.FirstName, DataType.NText),
                    new DataParameter("LAST_NAME", obj.LastName, DataType.NText),
                    new DataParameter("PASSWORD", obj.PasswordHash),
                    new DataParameter("AVATAR", obj.Avatar),
                };

            var fieldNames = string.Join(",", pp.Select(x => x.Name));
            var fieldValues = string.Join(",", pp.Select(x => ":" + x.Name));
            var sql = $"insert into {m_tableName} ({fieldNames}) values ({fieldValues})";

            var success = false;

            for (var i = 0; i < EntityCreateUtility.InsertAttemptsLimit; i++)
            {
                try
                {
                    db.Execute(sql, pp.ToArray());
                    success = true;
                    break;
                }
                catch (OracleException ex)
                {
                    //PK error. try one more time
                    if (ex.Number == 1 && ex.Message.Contains($"PK_{m_tableName}"))
                    {
                        if (i == EntityCreateUtility.InsertAttemptsLimit - 1)
                            throw;
                    }
                }
            }

            if (success)
            {
                // TODO: check generated queries
                var roles = UserRolesList(utcNow, newId, obj);
                db.BulkCopy(roles);
            }

            return Get(db, newId);
        }

        private static IEnumerable<USER_ROLE> UserRolesList(DateTime utcNow, uint id, User user)
        {
            if (user.IsOwner)
                yield return new USER_ROLE { ADD_TIMESTAMP = utcNow, ROLE_ID = (int)UserRoleCode.Owner, USER_ID = id };

            if (user.IsAdmin)
                yield return new USER_ROLE { ADD_TIMESTAMP = utcNow, ROLE_ID = (int)UserRoleCode.Admin, USER_ID = id };

            if (user.AgentDepartmentIds != null)
                foreach (var x in user.AgentDepartmentIds)
                    yield return
                        new USER_ROLE { ADD_TIMESTAMP = utcNow, ROLE_ID = (int)UserRoleCode.Agent, USER_ID = id, DEPARTMENT_ID = x };

            if (user.SupervisorDepartmentIds != null)
                foreach (var x in user.SupervisorDepartmentIds)
                    yield return
                        new USER_ROLE { ADD_TIMESTAMP = utcNow, ROLE_ID = (int)UserRoleCode.Supervisor, USER_ID = id, DEPARTMENT_ID = x };
        }

        public static User Update(ChatDatabase db, DateTime utcNow, uint id, UpdateInfo update)
        {
            var idParam = new DataParameter("ID", id);

            var pp = new List<DataParameter> { new DataParameter("UPDATE_TIMESTAMP", utcNow) };

            if (update.Status != null)
                pp.Add(new DataParameter("STATUS_ID", (int)update.Status.Value));
            if (update.IsOnline.HasValue)
                pp.Add(new DataParameter("IS_ONLINE", (sbyte)(update.IsOnline.Value ? 1 : 0)));
            if (update.Email != null)
                pp.Add(new DataParameter("EMAIL", update.Email, DataType.NText));
            if (update.FirstName != null)
                pp.Add(new DataParameter("FIRST_NAME", update.FirstName, DataType.NText));
            if (update.LastName != null)
                pp.Add(new DataParameter("LAST_NAME", update.LastName, DataType.NText));
            if (update.PasswordHash != null)
                pp.Add(new DataParameter("PASSWORD", update.PasswordHash));
            if (update.Avatar != null)
                pp.Add(new DataParameter("AVATAR", update.Avatar));

            var fieldUpdate = string.Join(",", pp.Select(x => x.Name + "=:" + x.Name));
            var sql = $"update {m_tableName} SET {fieldUpdate} WHERE ID=:ID";
            db.Execute(sql, pp.Concat(new[] { idParam }).ToArray());

            var currentRoles = db.USER_ROLE.Where(x => x.USER_ID == id).ToLookup(x => (UserRoleCode)x.ROLE_ID);

            var removeRoles = new List<USER_ROLE>();
            var createRoles = new List<USER_ROLE>();

            if (update.IsOwner.HasValue)
            {
                var hasRole = currentRoles[UserRoleCode.Owner].Any();
                var updateHasRole = update.IsOwner.Value;
                if (hasRole && !updateHasRole)
                    removeRoles.Add(new USER_ROLE { ROLE_ID = (int)UserRoleCode.Owner, USER_ID = id });
                else if (!hasRole && updateHasRole)
                    createRoles.Add(new USER_ROLE { ROLE_ID = (int)UserRoleCode.Owner, USER_ID = id, ADD_TIMESTAMP = utcNow });
            }

            if (update.IsAdmin.HasValue)
            {
                var hasRole = currentRoles[UserRoleCode.Admin].Any();
                var updateHasRole = update.IsAdmin.Value;
                if (hasRole && !updateHasRole)
                    removeRoles.Add(new USER_ROLE { ROLE_ID = (int)UserRoleCode.Admin, USER_ID = id });
                else if (!hasRole && updateHasRole)
                    createRoles.Add(new USER_ROLE { ROLE_ID = (int)UserRoleCode.Admin, USER_ID = id, ADD_TIMESTAMP = utcNow, });
            }

            if (update.AgentDepartments != null)
            {
                var hasDepartments = currentRoles[UserRoleCode.Agent]
                    .Where(x => x.DEPARTMENT_ID.HasValue)
                    .Select(x => x.DEPARTMENT_ID.Value)
                    .ToList();
                var remove = hasDepartments
                    .Where(x => !update.AgentDepartments.Contains(x))
                    .Select(x => new USER_ROLE { ROLE_ID = (int)UserRoleCode.Agent, USER_ID = id, DEPARTMENT_ID = x });
                removeRoles.AddRange(remove);
                var create = update.AgentDepartments
                    .Where(x => !hasDepartments.Contains(x))
                    .Select(x => new USER_ROLE { ROLE_ID = (int)UserRoleCode.Agent, USER_ID = id, DEPARTMENT_ID = x, ADD_TIMESTAMP = utcNow });
                createRoles.AddRange(create);
            }

            if (update.SupervisorDepartments != null)
            {
                var hasDepartments = currentRoles[UserRoleCode.Supervisor]
                    .Where(x => x.DEPARTMENT_ID.HasValue)
                    .Select(x => x.DEPARTMENT_ID.Value)
                    .ToList();
                var remove = hasDepartments
                    .Where(x => !update.SupervisorDepartments.Contains(x))
                    .Select(x => new USER_ROLE { ROLE_ID = (int)UserRoleCode.Supervisor, USER_ID = id, DEPARTMENT_ID = x });
                removeRoles.AddRange(remove);
                var create = update.SupervisorDepartments
                    .Where(x => !hasDepartments.Contains(x))
                    .Select(
                        x => new USER_ROLE { ROLE_ID = (int)UserRoleCode.Supervisor, USER_ID = id, DEPARTMENT_ID = x, ADD_TIMESTAMP = utcNow });
                createRoles.AddRange(create);
            }

            foreach (var r in removeRoles)
                db.USER_ROLE
                    .Where(x => x.USER_ID == r.USER_ID && x.ROLE_ID == r.ROLE_ID && x.DEPARTMENT_ID == r.DEPARTMENT_ID)
                    .Delete();
            db.BulkCopy(createRoles);

            return Get(db, id);
        }

        public class Validator : BusinessObjectValidatorBase
        {
            private const int EmailMaxLength = 256;
            private const int FirstNameMaxLength = 60;
            private const int LastNameMaxLength = 60;
            private const int PasswordHashMaxLength = 192;
            private const int AvatarMaxLength = 60;

            public List<ValidationMessage> ValidateNew(User user)
            {
                if (user == null) throw new ArgumentNullException(nameof(user));

                if (user.AgentDepartmentIds == null)
                    throw new ArgumentException("AgentDepartmentIds can't be null");
                if (user.SupervisorDepartmentIds == null)
                    throw new ArgumentException("SupervisorDepartmentIds can't be null");

                var messages = new List<ValidationMessage>();

                ValidateStringField(messages, "email", user.Email, false, EmailMaxLength, false);
                ValidateStringField(messages, "firstName", user.FirstName, false, FirstNameMaxLength, true);
                ValidateStringField(messages, "lastName", user.LastName, true, LastNameMaxLength, true);
                ValidateStringField(messages, "passwordHash", user.PasswordHash, false, PasswordHashMaxLength, false);
                ValidateStringField(messages, "avatar", user.Avatar, true, AvatarMaxLength, true);

                return messages;
            }

            public List<ValidationMessage> ValidateUpdate(UpdateInfo update)
            {
                if (update == null) throw new ArgumentNullException(nameof(update));

                var messages = new List<ValidationMessage>();

                ValidateStringField(messages, "email", update.Email, true, EmailMaxLength, false);
                ValidateStringField(messages, "firstName", update.FirstName, true, FirstNameMaxLength, true);
                ValidateStringField(messages, "lastName", update.LastName, true, LastNameMaxLength, true);
                ValidateStringField(messages, "passwordHash", update.PasswordHash, true, PasswordHashMaxLength, false);
                ValidateStringField(messages, "avatar", update.Avatar, true, AvatarMaxLength, true);

                return messages;
            }
        }

        public class UpdateInfo
        {
            public ObjectStatus? Status { get; set; }
            public bool? IsOnline { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PasswordHash { get; set; }
            public string Avatar { get; set; }
            public bool? IsOwner { get; set; }
            public bool? IsAdmin { get; set; }
            public HashSet<uint> AgentDepartments { get; set; }
            public HashSet<uint> SupervisorDepartments { get; set; }
        }
    }
}