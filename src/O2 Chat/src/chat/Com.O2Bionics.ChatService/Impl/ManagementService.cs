using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.MailerService;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.ChatService.Impl.AuditTrail.Names;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.ChatService.Objects.ChatEvents;
using Com.O2Bionics.ChatService.Properties;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.MailerService.Client;
using Com.O2Bionics.MailerService.Contract;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Jil;
using log4net;

namespace Com.O2Bionics.ChatService.Impl
{
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single,
        AddressFilterMode = AddressFilterMode.Any)]
    public class ManagementService : ChatServiceBase,
        IManagementService
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(ManagementService));

        private readonly IUserStorage m_userStorage;
        private readonly IUserManager m_userManager;
        private readonly IDepartmentStorage m_departmentStorage;
        private readonly IAccessManager m_accessManager;
        private readonly INowProvider m_nowProvider;
        private readonly IChatDatabaseFactory m_chatDatabaseFactory;
        private readonly IChatSessionManager m_chatSessionManager;
        private readonly ISubscriptionManager m_subscriptionManager;
        private readonly ICustomerStorage m_customerStorage;
        private readonly IMailerServiceClient m_emailSender;
        private readonly IFeatureServiceClient m_featureService;
        private readonly IChatWidgetAppearanceManager m_chatWidgetAppearanceManager;
        private readonly ChatServiceSettings m_chatServiceSettings;
        private readonly ICannedMessageStorage m_cannedMessageStorage;
        private readonly IAuditTrailClient m_auditTrailClient;
        private readonly IVisitorStorage m_visitorStorage;
        private readonly IChatSessionStorage m_chatSessionStorage;
        private readonly ICustomerWidgetLoadStorage m_customerWidgetLoadStorage;

        public ManagementService()
        {
        }

        public ManagementService(
            IUserManager userManager,
            IDepartmentStorage departmentStorage,
            IUserStorage userStorage,
            IAccessManager accessManager,
            ISubscriptionManager subscriptionManager,
            INowProvider nowProvider,
            IChatDatabaseFactory chatDatabaseFactory,
            IChatSessionManager chatSessionManager,
            ICustomerStorage customerStorage,
            IMailerServiceClient emailSender,
            IFeatureServiceClient featureService,
            IChatWidgetAppearanceManager chatWidgetAppearanceManager,
            ChatServiceSettings chatServiceSettings,
            IVisitorStorage visitorStorage,
            IChatSessionStorage chatSessionStorage,
            ICannedMessageStorage cannedMessageStorage,
            ICustomerWidgetLoadStorage customerWidgetLoadStorage,
            IAuditTrailClient auditTrailClient)
        {
            m_userManager = userManager;
            m_departmentStorage = departmentStorage;
            m_userStorage = userStorage;
            m_accessManager = accessManager;
            m_subscriptionManager = subscriptionManager;
            m_nowProvider = nowProvider;
            m_chatDatabaseFactory = chatDatabaseFactory;
            m_chatSessionManager = chatSessionManager;
            m_customerStorage = customerStorage;
            m_emailSender = emailSender;
            m_featureService = featureService;
            m_chatWidgetAppearanceManager = chatWidgetAppearanceManager;
            m_chatServiceSettings = chatServiceSettings;
            m_visitorStorage = visitorStorage;
            m_chatSessionStorage = chatSessionStorage;
            m_cannedMessageStorage = cannedMessageStorage;
            m_auditTrailClient = auditTrailClient ?? throw new ArgumentNullException(nameof(auditTrailClient));
            m_customerWidgetLoadStorage = customerWidgetLoadStorage ?? throw new ArgumentNullException(nameof(customerWidgetLoadStorage));
        }

        public UserInfo GetUserIdentity(uint customerId, uint userId)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, userId, });

                        return m_chatDatabaseFactory.Query(
                            db => m_userManager.GetUser(db, customerId, userId));
                    });
        }


        public GetUserResult GetUser(uint customerId, uint userId)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, userId, });

                        var areAvatarsAllowed = m_featureService.GetBool(customerId, FeatureCodes.Avatars).WaitAndUnwrapException();
                        var userInfo = m_chatDatabaseFactory.Query(
                            db => m_userManager.GetUser(db, customerId, userId));

                        return new GetUserResult
                            {
                                UserInfo = userInfo,
                                AreAvatarsAllowed = areAvatarsAllowed
                            };
                    });
        }

        public GetUsersResult GetUsers(uint adminUserId, uint customerId)
        {
            return HandleExceptions(
                s => new GetUsersResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { adminUserId, customerId, });

                        var areAvatarsAllowed = m_featureService.GetBool(customerId, FeatureCodes.Avatars).WaitAndUnwrapException();

                        return m_chatDatabaseFactory.Query(
                            db =>
                                {
                                    var user = m_userStorage.Get(db, customerId, adminUserId);
                                    m_accessManager.CheckUserStatus(user, customerId, adminUserId);

                                    var departments = m_departmentStorage
                                        .GetAll(db, customerId, false)
                                        .Select(x => x.AsInfo())
                                        .ToList();
                                    var users = m_userManager.GetUsers(db, customerId);
                                    var maxUsers = m_featureService.GetInt32(customerId, FeatureCodes.MaxUsers).WaitAndUnwrapException();
                                    return new GetUsersResult(
                                        users,
                                        departments,
                                        areAvatarsAllowed,
                                        maxUsers);
                                });
                    });
        }

        public UpdateUserResult CreateUser(uint adminUserId, UserInfo userInfo, string password)
        {
            return HandleExceptions(
                s => new UpdateUserResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { adminUserId, userInfo, password });

                        var auditEvent = CreateAuditEvent(
                            OperationKind.UserInsertKey,
                            userInfo.CustomerId,
                            adminUserId,
                            null,
                            userInfo);

                        UpdateUserResult QueryImpl(ChatDatabase db)
                        {
                            CheckCreateUserAccess(db, adminUserId, userInfo);

                            var maxUsers = m_featureService.GetInt32(userInfo.CustomerId, FeatureCodes.MaxUsers).WaitAndUnwrapException();
                            if (m_userManager.GetUsers(db, userInfo.CustomerId).Count >= maxUsers)
                            {
                                var message = new ValidationMessage(
                                    "Subscription Plan - MaxUsers",
                                    "Allowed users count is exceeded");
                                throw new ValidationException(message);
                            }

                            var created = m_userManager.CreateNew(db, userInfo, password);

                            auditEvent.NewValue = created;
                            var nameResolver = CreateNameResolver(db);
                            auditEvent.FetchDictionaries(nameResolver);

                            m_subscriptionManager.AgentEventSubscribers.Publish(
                                x => x.UserCreated(created.CustomerId, created));

                            return new UpdateUserResult(
                                new CallResultStatus(CallResultStatusCode.Success),
                                created);
                        }

                        var result = QueryAndTrackAuditEvent(auditEvent, QueryImpl);
                        return result;
                    });
        }

        private void CheckCreateUserAccess(ChatDatabase db, uint adminUserId, UserInfo userToBeCreated)
        {
            var customerId = userToBeCreated.CustomerId;

            var user = m_userStorage.Get(db, customerId, adminUserId);
            m_accessManager.CheckUserStatus(user, customerId, adminUserId);

            m_accessManager.CheckUserHasRoles(user, new UserRole(UserRoleCode.Admin));
            if (userToBeCreated.IsOwner)
                m_accessManager.CheckUserHasRoles(user, new UserRole(UserRoleCode.Owner));
        }

        public UpdateUserResult UpdateUser(uint adminUserId, UserInfo userInfo)
        {
            return HandleExceptions(
                s => new UpdateUserResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { adminUserId, userInfo });

                        var auditEvent = CreateAuditEvent(
                            OperationKind.UserUpdateKey,
                            userInfo.CustomerId,
                            adminUserId,
                            null,
                            userInfo);

                        UpdateUserResult QueryImpl(ChatDatabase db)
                        {
                            CheckUpdateUserAccess(db, adminUserId, userInfo.CustomerId, userInfo.Id);

                            var beforeUsers = m_userStorage.GetAll(db, userInfo.CustomerId);
                            var beforeUser = beforeUsers.FirstOrDefault(x => x.Id == userInfo.Id);
                            if (beforeUser == null)
                                throw new ValidationException(
                                    new ValidationMessage("userId", string.Format(Resources.UserNotFoundError1, userInfo.Id)));

                            auditEvent.OldValue = beforeUser.AsUserInfo();
                            var nameResolver = CreateNameResolver(db);
                            auditEvent.FetchDictionaries(nameResolver);

                            if (beforeUser.Status == ObjectStatus.Deleted)
                                throw new ValidationException(
                                    new ValidationMessage("userId", "Can't update deleted user for skey=" + userInfo.Id));

                            // can't disable last owner
                            if (beforeUser.IsOwner
                                && !userInfo.IsOwner
                                && !beforeUsers.Any(x => x.Id != userInfo.Id && x.Status == ObjectStatus.Active && x.IsOwner))
                                throw new ValidationException(
                                    new ValidationMessage("isOwner", "Can't remove owner role for the last active owner"));

                            // can't remove last owner's owner role
                            if (beforeUser.IsOwner
                                && beforeUser.Status != ObjectStatus.Disabled
                                && userInfo.Status == ObjectStatus.Disabled
                                && !beforeUsers.Any(x => x.Id != userInfo.Id && x.Status == ObjectStatus.Active && x.IsOwner))
                                throw new ValidationException(
                                    new ValidationMessage("isDisabled", "Can't disable the last active owner"));

                            var update = new User.UpdateInfo
                                {
                                    Status = userInfo.Status,
                                    Email = userInfo.Email,
                                    FirstName = userInfo.FirstName,
                                    LastName = userInfo.LastName,
                                    Avatar = userInfo.Avatar,
                                    IsOwner = userInfo.IsOwner,
                                    IsAdmin = userInfo.IsAdmin,
                                    AgentDepartments = userInfo.AgentDepartments,
                                    SupervisorDepartments = userInfo.SupervisorDepartments,
                                };
                            var updated = m_userManager.Update(db, userInfo.CustomerId, userInfo.Id, update);

                            auditEvent.NewValue = updated;
                            if (null != auditEvent.OldValue && null != auditEvent.NewValue)
                                auditEvent.FieldChanges = auditEvent.OldValue.Diff(auditEvent.NewValue, nameResolver);

                            m_subscriptionManager.AgentEventSubscribers
                                .Publish(x => x.UserUpdated(updated.CustomerId, updated));

                            return new UpdateUserResult(
                                new CallResultStatus(CallResultStatusCode.Success),
                                updated);
                        }

                        var result = QueryAndTrackAuditEvent(auditEvent, QueryImpl);
                        return result;
                    });
        }

        private NameResolver CreateNameResolver(ChatDatabase db) => new NameResolver(m_departmentStorage, db);

        private void CheckUpdateUserAccess(ChatDatabase db, uint adminUserId, uint customerId, uint updatingUserId)
        {
            var user = m_userStorage.Get(db, customerId, adminUserId);
            m_accessManager.CheckUserStatus(user, customerId, adminUserId);

            // user always can update himself
            if (updatingUserId == adminUserId)
                return;

            m_accessManager.CheckUserHasRoles(user, new UserRole(UserRoleCode.Admin));

            var updatingUser = m_userStorage.Get(db, customerId, updatingUserId);
            if (updatingUser != null && updatingUser.IsOwner)
                m_accessManager.CheckUserHasRoles(user, new UserRole(UserRoleCode.Owner));
        }

        public UpdateUserResult SetUserPassword(uint adminUserId, uint customerId, uint userId, string password)
        {
            return HandleExceptions(
                s => new UpdateUserResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { adminUserId, customerId, userId, password });

                        var auditEvent = CreateAuditEvent(
                            OperationKind.UserChangePasswordKey,
                            customerId,
                            adminUserId,
                            null,
                            new UserInfo
                                {
                                    CustomerId = customerId,
                                    Id = userId,
                                });

                        UpdateUserResult QueryImpl(ChatDatabase db)
                        {
                            CheckUpdateUserAccess(db, adminUserId, customerId, userId);

                            var update = new User.UpdateInfo
                                {
                                    PasswordHash = password.ToPasswordHash(),
                                };
                            var updated = m_userManager.Update(db, customerId, userId, update);

                            auditEvent.NewValue = updated;
                            var nameResolver = CreateNameResolver(db);
                            auditEvent.FetchDictionaries(nameResolver);

                            return new UpdateUserResult(
                                new CallResultStatus(CallResultStatusCode.Success),
                                updated);
                        }

                        var result = QueryAndTrackAuditEvent(auditEvent, QueryImpl);
                        return result;
                    });
        }

        public UpdateUserResult DeleteUser(uint adminUserId, uint customerId, uint userId)
        {
            return HandleExceptions(
                s => new UpdateUserResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { adminUserId, customerId, userId, });

                        var auditEvent = CreateAuditEvent(
                            OperationKind.UserDeleteKey,
                            customerId,
                            adminUserId,
                            new UserInfo
                                {
                                    CustomerId = customerId,
                                    Id = userId,
                                });

                        UpdateUserResult QueryImpl(ChatDatabase db)
                        {
                            CheckUpdateUserAccess(db, adminUserId, customerId, userId);

                            var beforeUsers = m_userStorage.GetAll(db, customerId);
                            var beforeUser = beforeUsers.FirstOrDefault(x => x.Id == userId);
                            if (beforeUser == null)
                                throw new ValidationException(
                                    new ValidationMessage("userId", string.Format(Resources.UserNotFoundError1, userId)));

                            auditEvent.OldValue = beforeUser.AsUserInfo();
                            var nameResolver = CreateNameResolver(db);
                            auditEvent.FetchDictionaries(nameResolver);

                            if (beforeUser.Status == ObjectStatus.Deleted)
                                throw new ValidationException(
                                    new ValidationMessage("userId", "Can't delete deleted user for id=" + userId));

                            // if user is the last owner - can't delete
                            if (beforeUser.IsOwner
                                && !beforeUsers.Any(x => x.Id != userId && x.Status == ObjectStatus.Active && x.IsOwner))
                                throw new ValidationException(
                                    new ValidationMessage("isOwner", "Can't delete the last active owner"));

                            var update = new User.UpdateInfo
                                {
                                    Status = ObjectStatus.Deleted,
                                };

                            m_userManager.Update(db, customerId, userId, update);

                            UnassignAgentSessions(customerId, adminUserId, userId);

                            m_subscriptionManager.AgentEventSubscribers.Publish(
                                x => x.UserRemoved(customerId, userId));

                            return new UpdateUserResult(
                                new CallResultStatus(CallResultStatusCode.Success));
                        }

                        var result = QueryAndTrackAuditEvent(auditEvent, QueryImpl);
                        return result;
                    });
        }

        public bool IsUserEmailExist(string email)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { email, });

                        return m_chatDatabaseFactory.Query(
                            db => m_userStorage.GetByEmail(db, email, false) != null);
                    });
        }

        private void UnassignAgentSessions(uint customerId, uint adminUserId, uint userId)
        {
            var utcNow = m_nowProvider.UtcNow;
            var sessions = m_chatSessionManager.GetAgentSessions(customerId, userId);
            foreach (var s in sessions.Where(x => x.Agents.Any(y => y.AgentId == userId)))
                try
                {
                    var ev = new AgentLeavesSessionChatEvent(
                        utcNow,
                        "Agent was removed by Administrator",
                        userId,
                        false,
                        false);
                    m_chatSessionManager.AddEvent(customerId, s.Skey, ev);
                }
                catch (Exception e)
                {
                    m_log.WarnFormat("Error removing Agent {0} from chat session {1}: {2}", userId, s.Skey, e);
                }

            foreach (var s in sessions.Where(
                x => x.Invites
                    .OfType<ChatSessionAgentInvite>()
                    .Any(y => y.IsPending && y.AgentId == userId)))
                try
                {
                    var ev = new AgentCancelsInviteAgentChatEvent(
                        utcNow,
                        "Agent was removed by Administrator",
                        adminUserId,
                        userId);
                    m_chatSessionManager.AddEvent(customerId, s.Skey, ev);
                }
                catch (Exception e)
                {
                    m_log.WarnFormat("Error canceling Agent {0} invite in chat session {1}: {2}", userId, s.Skey, e);
                }
        }


        public GetDepartmentsResult GetDepartments(uint adminUserId, uint customerId)
        {
            return HandleExceptions(
                s => new GetDepartmentsResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { adminUserId, customerId });

                        return m_chatDatabaseFactory.Query(
                            db =>
                                {
                                    var user = m_userStorage.Get(db, customerId, adminUserId);
                                    m_accessManager.CheckUserStatus(user, customerId, adminUserId);
                                    var maxDepartments = m_featureService.GetInt32(customerId, FeatureCodes.MaxDepartments).WaitAndUnwrapException();
                                    var list = m_departmentStorage.GetAll(db, customerId, false).Select(x => x.AsInfo()).ToList();
                                    return new GetDepartmentsResult(list, maxDepartments);
                                });
                    });
        }

        public UpdateDepartmentResult CreateDepartment(uint adminUserId, DepartmentInfo deptInfo)
        {
            return HandleExceptions(
                s => new UpdateDepartmentResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { adminUserId, deptInfo, });

                        var auditEvent = CreateAuditEvent(
                            OperationKind.DepartmentInsertKey,
                            deptInfo.CustomerId,
                            adminUserId,
                            null,
                            deptInfo);

                        UpdateDepartmentResult QueryImpl(ChatDatabase db)
                        {
                            var customerId = deptInfo.CustomerId;

                            var user = m_userStorage.Get(db, customerId, adminUserId);
                            m_accessManager.CheckUserStatus(user, customerId, adminUserId);
                            m_accessManager.CheckUserHasRoles(user, new UserRole(UserRoleCode.Admin));

                            var maxDepartments = m_featureService.GetInt32(customerId, FeatureCodes.MaxDepartments).WaitAndUnwrapException();

                            if (m_departmentStorage.GetAll(db, customerId, false).Count >= maxDepartments)
                                throw new ValidationException(
                                    new ValidationMessage("Subscription Plan - MaxDepartments", "Allowed departments count is exceeded"));

                            var dept = new Department(
                                customerId,
                                deptInfo.Name,
                                deptInfo.Description,
                                deptInfo.IsPublic);
                            dept = m_departmentStorage.CreateNew(db, dept);

                            deptInfo = dept.AsInfo();
                            auditEvent.NewValue = deptInfo;

                            m_subscriptionManager.AgentEventSubscribers.Publish(
                                x => x.DepartmentCreated(customerId, deptInfo));

                            return new UpdateDepartmentResult(
                                new CallResultStatus(CallResultStatusCode.Success),
                                deptInfo);
                        }

                        var result = QueryAndTrackAuditEvent(auditEvent, QueryImpl);
                        return result;
                    });
        }

        public UpdateDepartmentResult UpdateDepartment(uint adminUserId, DepartmentInfo deptInfo)
        {
            return HandleExceptions(
                s => new UpdateDepartmentResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { adminUserId, dept = deptInfo, });

                        var auditEvent = CreateAuditEvent(
                            OperationKind.DepartmentUpdateKey,
                            deptInfo.CustomerId,
                            adminUserId,
                            null,
                            deptInfo);

                        UpdateDepartmentResult QueryImpl(ChatDatabase db)
                        {
                            var customerId = deptInfo.CustomerId;

                            var user = m_userStorage.Get(db, customerId, adminUserId);
                            m_accessManager.CheckUserStatus(user, customerId, adminUserId);
                            m_accessManager.CheckUserHasRoles(user, new UserRole(UserRoleCode.Admin));

                            var beforeDept = m_departmentStorage.Get(db, customerId, deptInfo.Id);
                            if (beforeDept == null)
                                throw new ValidationException(
                                    new ValidationMessage("deptId", "Department not found for id=" + deptInfo.Id));
                            auditEvent.OldValue = beforeDept.AsInfo();

                            if (beforeDept.Status == ObjectStatus.Deleted)
                                throw new ValidationException(
                                    new ValidationMessage("deptId", "Can't update deleted department, id=" + deptInfo.Id));

                            var update = new Department.UpdateInfo
                                {
                                    Status = deptInfo.Status,
                                    IsPublic = deptInfo.IsPublic,
                                    Name = deptInfo.Name,
                                    Description = deptInfo.Description,
                                };
                            var dept = m_departmentStorage.Update(db, customerId, deptInfo.Id, update);

                            deptInfo = dept.AsInfo();
                            auditEvent.NewValue = deptInfo;
                            if (null != auditEvent.OldValue && null != auditEvent.NewValue)
                                auditEvent.FieldChanges = auditEvent.OldValue.Diff(auditEvent.NewValue);

                            m_subscriptionManager.AgentEventSubscribers
                                .Publish(x => x.DepartmentUpdated(customerId, deptInfo));

                            return new UpdateDepartmentResult(
                                new CallResultStatus(CallResultStatusCode.Success),
                                deptInfo);
                        }

                        var result = QueryAndTrackAuditEvent(auditEvent, QueryImpl);
                        return result;
                    });
        }

        public UpdateDepartmentResult DeleteDepartment(uint adminUserId, uint customerId, uint deptId)
        {
            return HandleExceptions(
                s => new UpdateDepartmentResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { adminUserId, customerId, deptId, });

                        var auditEvent = CreateAuditEvent(
                            OperationKind.DepartmentDeleteKey,
                            customerId,
                            adminUserId,
                            new DepartmentInfo
                                {
                                    CustomerId = customerId,
                                    Id = deptId,
                                });

                        UpdateDepartmentResult QueryImpl(ChatDatabase db)
                        {
                            var user = m_userStorage.Get(db, customerId, adminUserId);
                            m_accessManager.CheckUserStatus(user, customerId, adminUserId);
                            m_accessManager.CheckUserHasRoles(user, new UserRole(UserRoleCode.Admin));

                            var beforeDept = m_departmentStorage.Get(db, customerId, deptId);
                            if (beforeDept == null)
                                throw new ValidationException(
                                    new ValidationMessage("deptId", "Department not found for id=" + deptId));
                            auditEvent.OldValue = beforeDept.AsInfo();

                            if (beforeDept.Status == ObjectStatus.Deleted)
                                throw new ValidationException(
                                    new ValidationMessage("deptId", "Can't delete deleted department, id=" + deptId));

                            var update = new Department.UpdateInfo
                                {
                                    Status = ObjectStatus.Deleted,
                                };
                            m_departmentStorage.Update(db, customerId, deptId, update);

                            m_subscriptionManager.AgentEventSubscribers
                                .Publish(x => x.DepartmentRemoved(customerId, deptId));

                            return new UpdateDepartmentResult(
                                new CallResultStatus(CallResultStatusCode.Success));
                        }

                        var result = QueryAndTrackAuditEvent(auditEvent, QueryImpl);
                        return result;
                    });
        }

        public List<uint> GetCustomerIds()
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { });

                        return m_chatDatabaseFactory.Query(db => m_customerStorage.GetIds(db));
                    });
        }

        public CreateCustomerResult CreateCustomer(CreateCustomerParameters parameters)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { parameters });

                        UserInfo ownerUser = null;
                        Department department = null;

                        var result = m_chatDatabaseFactory.Query(
                            db =>
                                {
                                    try
                                    {
                                        var customer = m_customerStorage.CreateNew(
                                            db,
                                            new Customer(
                                                ObjectStatus.NotConfirmed,
                                                parameters.CustomerName,
                                                parameters.Domains,
                                                parameters.UserHostAddress));

                                        department = m_departmentStorage.CreateNew(
                                            db,
                                            new Department(
                                                customer.Id,
                                                "Default",
                                                "Default Department",
                                                true));

                                        ownerUser = m_userManager.CreateNew(
                                            db,
                                            new UserInfo
                                                {
                                                    CustomerId = customer.Id,
                                                    Status = ObjectStatus.NotConfirmed,
                                                    Email = parameters.Email,
                                                    FirstName = parameters.FirstName,
                                                    LastName = parameters.LastName,
                                                    Avatar = null,
                                                    IsOwner = true,
                                                    IsAdmin = true,
                                                    AgentDepartments = new HashSet<uint> { department.Id },
                                                    SupervisorDepartments = new HashSet<uint> { department.Id }
                                                },
                                            parameters.Password);

                                        // TODO: finish creating the customer.
                                        //
                                        //var template = new ConfirmCreateCustomerEmailTemplateEn();
                                        //template.Initialize();
                                        //var bodyHtml = template.TransformText();
                                        //var subject = template.Subject;
                                        //
                                        //m_emailSender.Send(parameters.Email, subject, bodyHtml);

                                        var result1 = new CreateCustomerResult(new CallResultStatus(CallResultStatusCode.Success), customer.AsInfo());
                                        return result1;
                                    }
                                    catch (ValidationException e)
                                    {
                                        db.RollbackTransaction();
                                        return new CreateCustomerResult(new CallResultStatus(CallResultStatusCode.ValidationFailed, e.Messages));
                                    }
                                });

                        LogNewCustomer(result, ownerUser, department);
                        return result;
                    });
        }

        private void LogNewCustomer(CreateCustomerResult result, UserInfo ownerUser, Department department)
        {
            try
            {
                if (result.Status.StatusCode != CallResultStatusCode.Success || null == ownerUser || null == department)
                    return;

                var auditEvent = CreateAuditEvent(
                    OperationKind.CustomerInsertKey,
                    result.Customer.Id,
                    ownerUser.Id,
                    null,
                    result.Customer);

                auditEvent.Author.Name = ownerUser.FullName();

                if (null == auditEvent.ObjectNames)
                    auditEvent.ObjectNames = new Dictionary<string, Dictionary<string, string>>();

                auditEvent.ObjectNames[EntityNames.Department] = new Dictionary<string, string>
                    {
                        { department.Id.ToString(CultureInfo.InvariantCulture), department.Name }
                    };

                SaveAuditEvent(auditEvent);
            }
            catch (Exception e)
            {
                try
                {
                    Log.Error("Save customer audit event", e);
                }
                catch (Exception)
                {
//Ignore
                }
            }
        }

        public SaveChatWidgetAppearanceJsonResult SaveChatWidgetAppearanceJson(
            uint customerId,
            uint adminUserId,
            ChatWidgetAppearance widgetAppearance,
            HashSet<string> beforeEditEnabledFeatures)
        {
            return HandleExceptions(
                s => new SaveChatWidgetAppearanceJsonResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, widgetAppearance, beforeEditEnabledFeatures, });

                        var auditEvent = CreateAuditEvent(
                            OperationKind.WidgetAppearanceUpdateKey,
                            customerId,
                            adminUserId,
                            null,
                            widgetAppearance);

                        SaveChatWidgetAppearanceJsonResult QueryImpl(ChatDatabase db)
                        {
                            var old = m_chatWidgetAppearanceManager.Get(customerId)?.AppearanceData;
                            auditEvent.OldValue = old;

                            m_chatWidgetAppearanceManager.Save(customerId, widgetAppearance);

                            if (null != auditEvent.OldValue && null != auditEvent.NewValue)
                                auditEvent.FieldChanges = auditEvent.OldValue.Diff(auditEvent.NewValue);

                            var enabledFeatures = m_chatWidgetAppearanceManager.GetEnabledFeatures(customerId);
                            if (!enabledFeatures.SetEquals(beforeEditEnabledFeatures))
                                return new SaveChatWidgetAppearanceJsonResult(
                                    new CallResultStatus(
                                        CallResultStatusCode.Warning,
                                        new ValidationMessage("", "Your Subscription has been changed. Please reload the page to synchronize.")));

                            return new SaveChatWidgetAppearanceJsonResult(
                                new CallResultStatus(CallResultStatusCode.Success));
                        }

                        var result = QueryAndTrackAuditEvent(auditEvent, QueryImpl);
                        return result;
                    });
        }

        public ChatWidgetAppearanceInfo GetChatWidgetAppearanceInfo(uint customerId)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, });

                        var customer = m_chatDatabaseFactory.Query(db => m_customerStorage.Get(db, customerId));
                        if (null == customer)
                            return null;

                        var chatWidgetAppearanceInfo = m_chatWidgetAppearanceManager.Get(customerId);
                        chatWidgetAppearanceInfo.Domains = customer.Domains;
                        return chatWidgetAppearanceInfo;
                    });
        }

        public UserLoginResult Login(LoginParameters loginParameters)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { loginParameters });

                        var result = m_chatDatabaseFactory.Query(db => m_userManager.AuthenticateUser(db, loginParameters));

                        TrackLoginEvent(result, loginParameters);

                        if (AccountLookupStatus.Success != result.Status)
                            result.User = null;

                        return result;
                    });
        }

        private void TrackLoginEvent([NotNull] UserLoginResult userLoginResult, [NotNull] LoginParameters loginParameters)
        {
            try
            {
                AuditEvent<UserInfo> auditEvent;
                if (null != userLoginResult.User)
                {
                    auditEvent = CreateAuditEvent(
                        OperationKind.UserLoginKey,
                        userLoginResult.User.CustomerId,
                        userLoginResult.User.Id,
                        null,
                        userLoginResult.User);

                    if (AccountLookupStatus.Success != userLoginResult.Status)
                    {
                        //TODO: read "DisableUserAfterFailedLoginsCountDefault" from task-302.
                        //var Todo_Task121_task302_attempts = 5;
                        var isSelfLocked = AccountLookupStatus.NotFound == userLoginResult.Status
                            //TODO: task-121 uncomment.
                            //&& Todo_Task121_task302_attempts == userLoginResult.User.FailedLogins
                            ;
                        auditEvent.Status = isSelfLocked ? OperationStatus.LoginFailedKey : userLoginResult.Status.ToString();
                    }

                    auditEvent.Author.Name = userLoginResult.User.FullName();
                }
                else
                {
                    auditEvent = new AuditEvent<UserInfo>
                        {
                            Status = OperationStatus.NotFoundKey,
                            Operation = OperationKind.UserLoginKey,
                            NewValue = new UserInfo
                                {
                                    Email = loginParameters.Email
                                }
                        };
                    auditEvent.SetContextCustomValues();
                }

                if (!string.IsNullOrEmpty(loginParameters.ClientType))
                    auditEvent.AddCustomValue(CustomFieldNames.ClientType, loginParameters.ClientType);
                if (!string.IsNullOrEmpty(loginParameters.ClientVersion))
                    auditEvent.AddCustomValue(CustomFieldNames.ClientVersion, loginParameters.ClientVersion);
                if (!string.IsNullOrEmpty(loginParameters.ClientLocalDate))
                    auditEvent.AddCustomValue(CustomFieldNames.ClientLocalDate, loginParameters.ClientLocalDate);
                if (!string.IsNullOrEmpty(loginParameters.ClientAddress))
                    auditEvent.AddCustomValue(CustomFieldNames.ClientIp, loginParameters.ClientAddress);

                SaveAuditEvent(auditEvent);
            }
            catch (Exception e)
            {
                try
                {
                    Log.Error("Save login audit event", e);
                }
                catch (Exception)
                {
//Ignore
                }
            }
        }

        public ResetPasswordResult ResetPassword(string code, string newPassword)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { code, newPassword });

                        return m_chatDatabaseFactory.Query(
                            db =>
                                m_userManager.ResetPassword(db, code, newPassword));
                    });
        }

        public CallResultStatus SendResetPasswordEmail(string email, string resetPasswordActionLink)
        {
            return HandleExceptions(
                s => s,
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { email, resetPasswordActionLink });

                        if (string.IsNullOrWhiteSpace(email))
                            throw new ValidationException(
                                new ValidationMessage("email", "Can't be null or whitespace"));

                        string code = null;
                        var accountLookupCode = m_chatDatabaseFactory.Query(
                            db => m_userManager.GenerateResetPasswordCode(db, email, out code));
                        if (accountLookupCode != AccountLookupStatus.Success)
                            throw new ValidationException(
                                new ValidationMessage("email", "This account has been removed or locked out."));

                        var templateModel = new { Link = resetPasswordActionLink + "?code=" + code };
                        var mailRequest = new MailRequest
                            {
                                ProductCode = ProductCodes.Chat,
                                TemplateId = TemplateIds.ResetPassword,
                                To = email,
                                TemplateModel = templateModel.JsonStringify2()
                            };
                        var error = m_emailSender.Send(mailRequest).WaitAndUnwrapException();
                        var result = string.IsNullOrEmpty(error)
                            ? new CallResultStatus(CallResultStatusCode.Success)
                            : new CallResultStatus(CallResultStatusCode.Failure, new ValidationMessage("email", error));
                        return result;
                    });
        }

        public List<WidgetViewStatisticsEntry> GetWidgetLoads(uint customerId, DateTime beginDate, DateTime endDate)
        {
            return HandleExceptions(
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, beginDate, endDate });

                        using (var context = m_chatDatabaseFactory.CreateContext())
                        {
                            var list = m_customerWidgetLoadStorage.GetForCustomer(context.Db, customerId, beginDate, endDate).ToList();
                            var result = 0 < list.Count ? list : null;
                            return result;
                        }
                    });
        }

        private static AuditEvent<T> CreateAuditEvent<T>(
            string operationKind,
            uint uintCustomerId,
            decimal authorId,
            [CanBeNull] T oldValue = null,
            [CanBeNull] T newValue = null)
            where T : class
        {
            var customerId = uintCustomerId.ToString(CultureInfo.InvariantCulture);
#if DEBUG
            if (string.IsNullOrEmpty(operationKind))
                throw new ArgumentNullException(nameof(operationKind));
            if (0 == uintCustomerId)
                throw new Exception($"{nameof(uintCustomerId)} must be set.");
            if (0 == authorId)
                throw new Exception($"{nameof(authorId)}({authorId}) must be set.");
#endif
            var result = new AuditEvent<T>
                {
                    Status = OperationStatus.SuccessKey,
                    Operation = operationKind,
                    Author = new Author(authorId),
                    CustomerId = customerId,
                    OldValue = oldValue,
                    NewValue = newValue
                };

            result.SetContextCustomValues();
            return result;
        }

        private void SetAuthor<T>(
            [NotNull] AuditEvent<T> auditEvent,
            [NotNull] ChatDatabase db)
            where T : class
        {
#if DEBUG
            if (string.IsNullOrEmpty(auditEvent.CustomerId))
                throw new Exception("auditEvent.CustomerId must be set");
            if (null == auditEvent.Author)
                throw new Exception("auditEvent.Author must be set");
            if (string.IsNullOrEmpty(auditEvent.Author.Id))
                throw new Exception("auditEvent.Author.Id must be set");
            if (!string.IsNullOrEmpty(auditEvent.Author.Name))
                throw new Exception("auditEvent.Author.Name is already set.");
#endif
            var customerId = uint.Parse(auditEvent.CustomerId);
            var userId = uint.Parse(auditEvent.Author.Id);

            var user = m_userStorage.Get(db, customerId, userId);
            if (null == user)
                throw new CallResultException(
                    CallResultStatusCode.AccessDenied,
                    new ValidationMessage("userId", string.Format(Resources.UserNotFoundError1, userId)));

            auditEvent.Author.Name = user.FullName();
        }

        private TResult QueryAndTrackAuditEvent<T, TResult>(
            [NotNull] AuditEvent<T> auditEvent,
            [NotNull] Func<ChatDatabase, TResult> func)
            where T : class
        {
            try
            {
                var result = m_chatDatabaseFactory.Query(
                    db =>
                        {
                            SetAuthor(auditEvent, db);
                            return func(db);
                        });
                return result;
            }
            catch (Exception e)
            {
                auditEvent.SetExceptionAndStatus(e);
                throw;
            }
            finally
            {
                SaveAuditEvent(auditEvent);
            }
        }

        private void SaveAuditEvent<T>(AuditEvent<T> auditEvent)
            where T : class
        {
            try
            {
                auditEvent.SetAnalyzedFields();

                m_auditTrailClient.Save(auditEvent).WaitAndUnwrapException();
            }
            catch (Exception e)
            {
                try
                {
                    try
                    {
                        var serialized = JSON.Serialize(auditEvent, JsonSerializerBuilder.SkipNullJilOptions);
                        m_log.Error($"Audit trail save failed, ProductCode={ProductCodes.Chat}, auditEvent='{serialized}'.", e);
                    }
                    catch
                    {
                        m_log.Error("Audit trail save.", e);
                    }
                }
                catch
                {
                    //Ignore
                }
            }
        }

        #region Canned Messages

        public UpdateCannedMessageResult CreateNewCannedMessage(uint customerId, uint currentUserId, CannedMessageInfo info)
        {
            return HandleExceptions(
                s => new UpdateCannedMessageResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, currentUserId, info });

                        var created = m_chatDatabaseFactory.Query(
                            db =>
                                {
                                    var user = m_userStorage.Get(db, customerId, currentUserId);
                                    m_accessManager.CheckUserStatus(user, customerId, currentUserId);

                                    if (info.DepartmentId.HasValue)
                                    {
                                        m_accessManager.CheckUserHasRoles(user, new UserRole(UserRoleCode.Admin));

                                        var dept = m_departmentStorage.Get(db, customerId, info.DepartmentId.Value);
                                        if (dept == null)
                                        {
                                            throw new CallResultException(
                                                CallResultStatusCode.AccessDenied,
                                                new ValidationMessage("departmentId", "Invalid department id"));
                                        }
                                    }

                                    if (info.UserId.HasValue && info.UserId != currentUserId)
                                    {
                                        throw new CallResultException(
                                            CallResultStatusCode.AccessDenied,
                                            new ValidationMessage("userId", "Can't add CM to other user"));
                                    }

                                    return m_cannedMessageStorage.CreateNew(
                                        db,
                                        customerId,
                                        new CannedMessage(
                                            info.UserId,
                                            info.DepartmentId,
                                            info.Key,
                                            info.Value));
                                });

                        return new UpdateCannedMessageResult(created.AsInfo());
                    });
        }

        public UpdateCannedMessageResult UpdateCannedMessage(
            uint customerId,
            uint currentUserId,
            uint id,
            CannedMessageInfo updateInfo)
        {
            return HandleExceptions(
                s => new UpdateCannedMessageResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, currentUserId, id, updateInfo });

                        var updated = m_chatDatabaseFactory.Query(
                            db =>
                                {
                                    var user = m_userStorage.Get(db, customerId, currentUserId);
                                    m_accessManager.CheckUserStatus(user, customerId, currentUserId);

                                    var subj = m_cannedMessageStorage.Get(db, customerId, id);
                                    if (subj == null)
                                    {
                                        throw new CallResultException(CallResultStatusCode.NotFound);
                                    }

                                    if (subj.DepartmentId.HasValue)
                                    {
                                        m_accessManager.CheckUserHasRoles(user, new UserRole(UserRoleCode.Admin));
                                    }

                                    if (subj.UserId.HasValue && subj.UserId != currentUserId)
                                    {
                                        throw new CallResultException(
                                            CallResultStatusCode.AccessDenied,
                                            new ValidationMessage("userId", "Can't change userId"));
                                    }

                                    var update = new CannedMessage.UpdateInfo
                                        {
                                            MessageKey = updateInfo.Key,
                                            MessageValue = updateInfo.Value
                                        };
                                    return m_cannedMessageStorage.Update(db, customerId, id, update);
                                });

                        return new UpdateCannedMessageResult(updated.AsInfo());
                    });
        }

        public UpdateCannedMessageResult DeleteCannedMessage(uint customerId, uint currentUserId, uint id)
        {
            return HandleExceptions(
                s => new UpdateCannedMessageResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, currentUserId, id });

                        m_chatDatabaseFactory.Query(
                            db =>
                                {
                                    var user = m_userStorage.Get(db, customerId, currentUserId);
                                    m_accessManager.CheckUserStatus(user, customerId, currentUserId);

                                    var subj = m_cannedMessageStorage.Get(db, customerId, id);
                                    if (subj == null)
                                    {
                                        throw new CallResultException(CallResultStatusCode.NotFound);
                                    }

                                    if (subj.DepartmentId.HasValue)
                                    {
                                        m_accessManager.CheckUserHasRoles(user, new UserRole(UserRoleCode.Admin));
                                    }

                                    if (subj.UserId.HasValue && subj.UserId != currentUserId)
                                    {
                                        throw new CallResultException(
                                            CallResultStatusCode.AccessDenied,
                                            new ValidationMessage("userId", "Can't delete other user's object"));
                                    }

                                    m_cannedMessageStorage.Delete(db, customerId, id);
                                });

                        return new UpdateCannedMessageResult(new CallResultStatus(CallResultStatusCode.Success));
                    });
        }

        public GetCannedMessagesResult GetUserCannedMessages(uint customerId, uint currentUserId)
        {
            return HandleExceptions(
                s => new GetCannedMessagesResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, currentUserId });

                        var list = m_chatDatabaseFactory.Query(
                            db =>
                                {
                                    var user = m_userStorage.Get(db, customerId, currentUserId);
                                    m_accessManager.CheckUserStatus(user, customerId, currentUserId);

                                    var userDepartments = user.AgentDepartmentIds;

                                    return m_cannedMessageStorage.GetMany(
                                        db,
                                        customerId,
                                        currentUserId,
                                        userDepartments);
                                });

                        return new GetCannedMessagesResult(list.Select(x => x.AsInfo()).ToList());
                    });
        }

        public GetCannedMessagesResult GetDepartmentCannedMessages(uint customerId, uint currentUserId, uint departmentId)
        {
            return HandleExceptions(
                s => new GetCannedMessagesResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, currentUserId, departmentId });

                        var list = m_chatDatabaseFactory.Query(
                            db =>
                                {
                                    var user = m_userStorage.Get(db, customerId, currentUserId);
                                    m_accessManager.CheckUserStatus(user, customerId, currentUserId);
                                    m_accessManager.CheckUserHasRoles(user, new UserRole(UserRoleCode.Admin));

                                    return m_cannedMessageStorage.GetMany(
                                        db,
                                        customerId,
                                        null,
                                        new HashSet<uint> { departmentId });
                                });

                        return new GetCannedMessagesResult(list.Select(x => x.AsInfo()).ToList());
                    });
        }

        #endregion

        #region Get Data Chat History

        public SessionSearchResult GetSessions(
            uint customerId,
            uint currentUserId,
            SessionSearchFilter filter,
            int pageSize,
            int pageNumber)
        {
            return HandleExceptions(
                s => new SessionSearchResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, currentUserId, filter, pageSize, pageNumber });

                        var visibleDepartments = GetUserSessionAccess(customerId, currentUserId);

                        var sessions = m_chatSessionStorage.Search(
                                customerId,
                                currentUserId,
                                visibleDepartments,
                                filter,
                                pageSize,
                                pageNumber)
                            .Select(x => x.AsInfo())
                            .ToList();
                        var visitors = sessions
                            .Take(sessions.Count - 1)
                            .Where(x => x.VisitorId.HasValue)
                            .Select(x => x.VisitorId.Value)
                            .Distinct()
                            .Select(x => m_visitorStorage.Get(x)?.AsInfo())
                            .Where(x => x != null)
                            .ToList();
                        return new SessionSearchResult(pageSize, sessions, visitors);
                    });
        }

        public GetSessionResult GetSession(
            uint customerId,
            uint currentUserId,
            long sessionSkey,
            int messagesPageSize)
        {
            return HandleExceptions(
                s => new GetSessionResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, currentUserId, sessionSkey, messagesPageSize });

                        var visibleDepartments = GetUserSessionAccess(customerId, currentUserId);
                        var session = CheckSessionAccess(customerId, currentUserId, sessionSkey, visibleDepartments);

                        var messages = m_chatSessionStorage.GetMessages(customerId, sessionSkey, messagesPageSize, 1);
                        var messagesInfo = new GetSessionMessagesResult(messagesPageSize, messages.Select(x => x.AsInfo()).ToList());

                        var visitorInfo = session.VisitorId != null
                            ? m_visitorStorage.Get(session.VisitorId.Value)?.AsInfo()
                            : null;

                        var qr = m_chatDatabaseFactory.Query(
                            db => new
                                {
                                    users = m_userStorage.GetAll(db, customerId),
                                    departments = m_departmentStorage.GetAll(db, customerId, false)
                                });

                        var result = new GetSessionResult(
                            session.AsInfo(),
                            messagesInfo,
                            visitorInfo,
                            qr.users.Select(x => x.AsUserInfo()).ToList(),
                            qr.departments.Select(x => x.AsInfo()).ToList());
                        return result;
                    });
        }

        public GetSessionMessagesResult GetSessionMessages(
            uint customerId,
            uint currentUserId,
            long sessionSkey,
            int pageSize,
            int pageNumber)
        {
            return HandleExceptions(
                s => new GetSessionMessagesResult(s),
                () =>
                    {
                        if (Log.IsDebugEnabled)
                            LogMethodCall(new { customerId, currentUserId, sessionSkey, pageNumber, pageSize });

                        var visibleDepartments = GetUserSessionAccess(customerId, currentUserId);
                        CheckSessionAccess(customerId, currentUserId, sessionSkey, visibleDepartments);

                        var messages = m_chatSessionStorage.GetMessages(
                            customerId,
                            sessionSkey,
                            pageSize,
                            pageNumber);

                        return new GetSessionMessagesResult(pageSize, messages.Select(x => x.AsInfo()).ToList());
                    });
        }

        private ChatSession CheckSessionAccess(
            uint customerId,
            uint currentUserId,
            long sessionSkey,
            HashSet<uint> visibleDepartments)
        {
            var session = m_chatSessionStorage.Get(customerId, sessionSkey);
            if (session == null)
                throw new CallResultException(CallResultStatusCode.NotFound);
            if (!session.AgentsInvolved.Contains(currentUserId)
                && !session.DepartmentsInvolved.Overlaps(visibleDepartments))
                throw new CallResultException(CallResultStatusCode.AccessDenied);
            return session;
        }

        private HashSet<uint> GetUserSessionAccess(uint customerId, uint currentUserId)
        {
            var user = m_chatDatabaseFactory.Query(
                db => m_userStorage.Get(db, customerId, currentUserId));
            m_accessManager.CheckUserStatus(user, customerId, currentUserId);

            if (!user.IsInRole(UserRoleCode.Supervisor) && !user.IsInRole(UserRoleCode.Agent))
                throw new CallResultException(CallResultStatusCode.AccessDenied);

            var visibleDepartments = new HashSet<uint>();
            if (user.IsInRole(UserRoleCode.Supervisor))
            {
                visibleDepartments.UnionWith(user.SupervisorDepartmentIds);
            }

            if (user.IsInRole(UserRoleCode.Agent))
            {
                visibleDepartments.UnionWith(user.AgentDepartmentIds);
            }

            return visibleDepartments;
        }

        #endregion
    }
}