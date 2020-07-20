using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.Utils;
using log4net;

namespace Com.O2Bionics.ChatService.Impl
{
    public class UserManager : IUserManager
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(UserManager));

        private readonly INowProvider m_nowProvider;
        private readonly ISettingsStorage m_settingsStorage;
        private readonly IUserStorage m_userStorage;
        private readonly ICustomerStorage m_customerStorage;
        private readonly IFeatureServiceClient m_featureService;

        public UserManager(
            INowProvider nowProvider,
            ISettingsStorage settingsStorage,
            IUserStorage userStorage,
            ICustomerStorage customerStorage,
            IFeatureServiceClient featureService)
        {
            m_nowProvider = nowProvider;
            m_settingsStorage = settingsStorage;
            m_userStorage = userStorage;
            m_customerStorage = customerStorage;
            m_featureService = featureService;
        }

        public List<UserInfo> GetUsers(ChatDatabase db, uint customerId, HashSet<uint> ids = null)
        {
            var all = m_userStorage.GetAll(db, customerId).AsQueryable();
            if (ids != null)
                all = all.Where(x => ids.Contains(x.Id));
            var allInfo = all.Select(x => x.AsUserInfo()).ToList();
            if (!AreAvatarsEnabled(customerId))
                foreach (var x in allInfo)
                    x.Avatar = null;
            return allInfo;
        }

        public List<AgentInfo> GetAgents(ChatDatabase db, uint customerId)
        {
            var allInfo = m_userStorage.GetAll(db, customerId)
                .Where(x => x.AgentDepartmentIds.Count > 0)
                .Select(x => x.AsInfo())
                .ToList();
            if (!AreAvatarsEnabled(customerId))
                foreach (var x in allInfo)
                    x.Avatar = null;
            return allInfo;
        }

        public AgentInfo GetAgent(ChatDatabase db, uint customerId, uint id)
        {
            var info = m_userStorage.Get(db, customerId, id)?.AsInfo();
            if (info != null && !AreAvatarsEnabled(customerId)) info.Avatar = null;
            return info;
        }

        public UserInfo GetUser(ChatDatabase db, uint customerId, uint id)
        {
            var info = m_userStorage.Get(db, customerId, id)?.AsUserInfo();
            if (info != null && !AreAvatarsEnabled(customerId)) info.Avatar = null;
            return info;
        }

        public UserInfo Update(ChatDatabase db, uint customerId, uint userId, User.UpdateInfo update)
        {
            var areAvatarsEnabled = AreAvatarsEnabled(customerId);
            update.Avatar = areAvatarsEnabled ? NormalizeAvatar(update.Avatar, userId) : null;

            var updated = m_userStorage.Update(db, customerId, userId, update);

            if (updated == null)
                return null;

            var updatedUserInfo = updated.AsUserInfo();

            if (!areAvatarsEnabled) updatedUserInfo.Avatar = null;
            return updatedUserInfo;
        }

        public UserInfo CreateNew(ChatDatabase db, UserInfo create, string password)
        {
            var areAvatarsEnabled = AreAvatarsEnabled(create.CustomerId);
            var avatar = areAvatarsEnabled ? NormalizeAvatar(create.Avatar, 0u) : null;

            var user = new User(
                create.CustomerId,
                create.Status,
                create.Email,
                create.FirstName,
                create.LastName,
                password.ToPasswordHash(),
                avatar,
                create.IsOwner,
                create.IsAdmin,
                create.AgentDepartments ?? new HashSet<uint>(),
                create.SupervisorDepartments ?? new HashSet<uint>());
            return m_userStorage.CreateNew(db, user).AsUserInfo();
        }

        #region login related

        public UserLoginResult AuthenticateUser(ChatDatabase db, LoginParameters loginParams)
        {
            // ReSharper disable NotResolvedInText
            if (loginParams == null)
                throw new ArgumentNullException(nameof(loginParams));
            if (string.IsNullOrWhiteSpace(loginParams.Email))
                throw new ArgumentException("Can't be null or whitespace", "loginParams.Email");
            if (loginParams.Password == null)
                throw new ArgumentException("Can't be null", "loginParams.PasswordHash");
            if (string.IsNullOrWhiteSpace(loginParams.ClientType))
                throw new ArgumentException("Can't be null or whitespace", "loginParams.ClientType");
            if (string.IsNullOrWhiteSpace(loginParams.ClientVersion))
                throw new ArgumentException("Can't be null or whitespace", "loginParams.ClientVersion");
            if (string.IsNullOrWhiteSpace(loginParams.ClientAddress))
                throw new ArgumentException("Can't be null or whitespace", "loginParams.ClientAddress");
            if (loginParams.ClientLocalDate == null)
                throw new ArgumentException("Can't be null", "loginParams.ClientLocalDate");
            // ReSharper restore NotResolvedInText

            m_log.DebugFormat(
                "AuthenticateUser: email={0} address={1} client={2}/{3}",
                loginParams.Email,
                loginParams.ClientAddress,
                loginParams.ClientType,
                loginParams.ClientVersion);

            var user = m_userStorage.GetByEmail(db, loginParams.Email, false);
            if (user == null)
            {
                m_log.WarnFormat("AuthenticateUser: email={0} - user not found", loginParams.Email);
                return new UserLoginResult { Status = AccountLookupStatus.NotFound };
            }

            var userInfo = user.AsUserInfo();
            if (!AreAvatarsEnabled(user.CustomerId)) userInfo.Avatar = null;

            var customer = m_customerStorage.Get(db, user.CustomerId);
            if (null == customer || customer.Status != ObjectStatus.Active)
            {
                return new UserLoginResult { Status = AccountLookupStatus.CustomerNotActive, User = userInfo };
            }

            if (user.Status == ObjectStatus.Disabled)
            {
                m_log.WarnFormat("AuthenticateUser: email={0} - user is disabled", loginParams.Email);
                return new UserLoginResult { Status = AccountLookupStatus.NotActive, User = userInfo };
            }

            if (user.Status == ObjectStatus.NotConfirmed)
            {
                m_log.WarnFormat("AuthenticateUser: email={0} - user's email isn't confirmed", loginParams.Email);
                return new UserLoginResult { Status = AccountLookupStatus.NotActive, User = userInfo };
            }

            var passwordHash = loginParams.Password.ToPasswordHash();
            if (user.PasswordHash != passwordHash)
            {
                m_log.WarnFormat("AuthenticateUser: email={0} - wrong password", loginParams.Email);
                return new UserLoginResult { Status = AccountLookupStatus.NotFound, User = userInfo };
            }

            m_log.DebugFormat("AuthenticateUser: email={0} - success", loginParams.Email);

            return new UserLoginResult
                {
                    Status = AccountLookupStatus.Success,
                    User = userInfo
                };
        }

        public AccountLookupStatus GenerateResetPasswordCode(ChatDatabase db, string email, out string code)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Can't be null or whitespace", nameof(email));

            m_log.DebugFormat("GenerateResetPasswordCode for email={0}", email);

            code = null;
            var user = m_userStorage.GetByEmail(db, email, true);
            if (user == null)
            {
                m_log.WarnFormat("GenerateResetPasswordCode: no active user accounts found for email={0}", email);
                return AccountLookupStatus.NotFound;
            }

            if (user.Status != ObjectStatus.Active)
            {
                m_log.WarnFormat("GenerateResetPasswordCode: user isn't active for email={0}: {1:G}", email, user.Status);
                return AccountLookupStatus.NotActive;
            }

            code = GenerateCode();
            m_userStorage.RecordResetPasswordCode(db, user.Id, user.Email, code);

            m_log.DebugFormat(
                "GenerateResetPasswordCode: code generated and stored for skey={0} email={1}: code={2}",
                user.Id,
                user.Email,
                code);
            return AccountLookupStatus.Success;
        }

        private static string GenerateCode()
        {
            return StringEncryptor.Encrypt(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"));
        }

        public ResetPasswordResult ResetPassword(ChatDatabase db, string code, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Can't be null or whitespace", nameof(code));
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Can't be null or whitespace", nameof(newPassword));

            m_log.DebugFormat("ResetPassword for code={0}", code);

            var minTimestamp = m_nowProvider.UtcNow - m_settingsStorage.GetServiceSettings().PasswordResetCodeTimeout;

            var userIds = m_userStorage.GetResetPasswordCodeUserId(db, minTimestamp, code);
            if (userIds == null)
            {
                m_log.WarnFormat("ResetPassword for code={0} - no records found", code);
                return new ResetPasswordResult { Status = ResetPasswordCodeStatus.CodeNotFoundOrExpired };
            }

            m_log.DebugFormat("ResetPassword for code={0} - found id={1}", code, userIds);
            var user = m_userStorage.Get(db, userIds.Item1.Value, userIds.Item2.Value);
            if (user == null || user.Status != ObjectStatus.Active)
            {
                m_log.WarnFormat("ResetPassword for code={0} - active agent accounts not found for email={1}", code, userIds);
                return new ResetPasswordResult { Status = ResetPasswordCodeStatus.AccountRemovedOrLocked };
            }

            m_userStorage.Update(db, user.CustomerId, user.Id, new User.UpdateInfo { PasswordHash = newPassword.ToPasswordHash() });
            m_userStorage.DeleteResetPasswordCode(db, code);

            m_log.DebugFormat("ResetPassword for code={0} - password updated for userId={1}", code, userIds);
            return new ResetPasswordResult { Email = user.Email, Status = ResetPasswordCodeStatus.Success };
        }

        #endregion

        private string NormalizeAvatar(string avatar, uint userId)
        {
            // userInfo.Avatar = Constants.DefaultAvatarPrefix + System.IO.Path.GetFileName(userInfo.Avatar);
            if (avatar == null) return null;
            avatar = avatar.Trim();
            if (avatar.Length == 0) return null;

            if (avatar.StartsWith(AvatarConstants.DefaultAvatarPrefix, StringComparison.Ordinal))
            {
                return AvatarConstants.DefaultAvatarPrefix
                       + Path.GetFileName(avatar.Substring(AvatarConstants.DefaultAvatarPrefix.Length));
            }

            m_log.WarnFormat("Invalid avatar schema: user={0}, '{1}', resetting avatar.", userId, avatar);
            return null;
        }

        private bool AreAvatarsEnabled(uint customerId)
        {
            return m_featureService.GetBool(customerId, FeatureCodes.Avatars).WaitAndUnwrapException();
        }
    }
}