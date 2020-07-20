using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService
{
    public interface IUserManager
    {
        List<AgentInfo> GetAgents(ChatDatabase db, uint customerId);
        AgentInfo GetAgent(ChatDatabase db, uint customerId, uint id);

        [NotNull]
        List<UserInfo> GetUsers(ChatDatabase db, uint customerId, HashSet<uint> ids = null);

        UserInfo GetUser(ChatDatabase db, uint customerId, uint id);
        UserInfo Update(ChatDatabase db, uint customerId, uint userId, User.UpdateInfo update);
        UserInfo CreateNew(ChatDatabase db, UserInfo create, string password);

        UserLoginResult AuthenticateUser(ChatDatabase db, LoginParameters loginParams);
        AccountLookupStatus GenerateResetPasswordCode(ChatDatabase db, string email, out string code);
        ResetPasswordResult ResetPassword(ChatDatabase db, string code, string newPassword);
    }
}