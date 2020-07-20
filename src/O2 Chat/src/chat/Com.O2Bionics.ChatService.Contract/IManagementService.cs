using System;
using System.Collections.Generic;
using System.ServiceModel;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Contract
{
    [ServiceContract(Namespace = ServiceConstants.Namespace)]
    public interface IManagementService
#if ERRORTRACKERTEST
        : IErrorTrackerTest
#endif
    {
        [OperationContract]
        CreateCustomerResult CreateCustomer(CreateCustomerParameters parameters);

        [OperationContract]
        List<uint> GetCustomerIds();

        [OperationContract]
        SaveChatWidgetAppearanceJsonResult SaveChatWidgetAppearanceJson(
            uint customerId,
            uint adminUserId,
            ChatWidgetAppearance widgetAppearance,
            HashSet<string> beforeEditEnabledFeatures);

        [OperationContract]
        [CanBeNull]
        ChatWidgetAppearanceInfo GetChatWidgetAppearanceInfo(uint customerId);


        [OperationContract]
        UserLoginResult Login(LoginParameters loginParameters);

        [OperationContract]
        ResetPasswordResult ResetPassword(string code, string newPassword);


        [OperationContract]
        GetDepartmentsResult GetDepartments(uint adminUserId, uint customerId);

        [OperationContract]
        UpdateDepartmentResult CreateDepartment(uint adminUserId, DepartmentInfo dept);

        [OperationContract]
        UpdateDepartmentResult UpdateDepartment(uint adminUserId, DepartmentInfo deptInfo);

        [OperationContract]
        UpdateDepartmentResult DeleteDepartment(uint adminUserId, uint customerId, uint deptId);


        [OperationContract]
        UserInfo GetUserIdentity(uint customerId, uint userId);

        [OperationContract]
        GetUserResult GetUser(uint customerId, uint userId);

        [OperationContract]
        GetUsersResult GetUsers(uint adminUserId, uint customerId);

        [OperationContract]
        UpdateUserResult CreateUser(uint adminUserId, UserInfo user, string password);

        [OperationContract]
        UpdateUserResult UpdateUser(uint adminUserId, UserInfo user);

        [OperationContract]
        UpdateUserResult SetUserPassword(uint adminUserId, uint customerId, uint userId, string password);

        [OperationContract]
        UpdateUserResult DeleteUser(uint adminUserId, uint customerId, uint userId);

        [OperationContract]
        bool IsUserEmailExist(string email);

        [OperationContract]
        CallResultStatus SendResetPasswordEmail(string email, string resetPasswordActionLink);

        [OperationContract]
        [CanBeNull]
        List<WidgetViewStatisticsEntry> GetWidgetLoads(uint customerId, DateTime beginDate, DateTime endDate);

        #region Canned Messages

        [OperationContract]
        UpdateCannedMessageResult CreateNewCannedMessage(uint customerId, uint currentUserId, CannedMessageInfo info);

        [OperationContract]
        UpdateCannedMessageResult DeleteCannedMessage(uint customerId, uint currentUserId, uint id);

        [OperationContract]
        UpdateCannedMessageResult UpdateCannedMessage(uint customerId, uint currentUserId, uint id, CannedMessageInfo updateInfo);

        [OperationContract]
        GetCannedMessagesResult GetUserCannedMessages(uint customerId, uint currentUserId);

        [OperationContract]
        GetCannedMessagesResult GetDepartmentCannedMessages(uint customerId, uint currentUserId, uint departmentId);

        #endregion

        #region Get Data Chat History

        [OperationContract]
        SessionSearchResult GetSessions(
            uint customerId,
            uint currentUserId,
            SessionSearchFilter filter,
            int pageSize,
            int pageNumber);

        [OperationContract]
        GetSessionResult GetSession(
            uint customerId,
            uint currentUserId,
            long sessionSkey,
            int messagesPageSize);

        [OperationContract]
        GetSessionMessagesResult GetSessionMessages(
            uint customerId,
            uint currentUserId,
            long sessionSkey,
            int pageSize,
            int pageNumber);

        #endregion
    }
}