using System.Collections.Generic;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;

namespace Com.O2Bionics.ChatService
{
    public interface ICannedMessageStorage
    {
        CannedMessage CreateNew(ChatDatabase db, uint customerId, CannedMessage cannedMessage);
        void Delete(ChatDatabase db, uint customerId, uint id);
        CannedMessage Update(ChatDatabase db, uint customerId, uint id, CannedMessage.UpdateInfo update);
        List<CannedMessage> GetMany(ChatDatabase db, uint customerId, decimal? userId, HashSet<uint> departments);
        CannedMessage Get(ChatDatabase db, uint customerId, uint id);
    }
}