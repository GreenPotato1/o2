using System;
using System.Collections.Generic;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;

namespace Com.O2Bionics.ChatService
{
    public interface IUserStorage
    {
        List<User> GetAll(ChatDatabase db, uint customerId);
        User Get(ChatDatabase db, uint customerId, uint id);
        User GetByEmail(ChatDatabase db, string email, bool skipDisabled);
        HashSet<uint> GetCustomerAgentsDepartments(ChatDatabase db, uint customerId, HashSet<uint> agentIds);
        HashSet<uint> GetOnline(ChatDatabase db, uint customerId);

        User CreateNew(ChatDatabase db, User user);
        User Update(ChatDatabase db, uint customerId, uint id, User.UpdateInfo update, bool resetCache = false);

        void RecordResetPasswordCode(ChatDatabase db, uint userId, string email, string code);
        void DeleteResetPasswordCode(ChatDatabase db, string code);
        Tuple<uint?, uint?> GetResetPasswordCodeUserId(ChatDatabase db, DateTime minTimestamp, string code);
    }
}