using System.Collections.Generic;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService
{
    public interface ICustomerStorage
    {
        [CanBeNull]
        Customer Get(ChatDatabase db, uint id);

        Customer CreateNew(ChatDatabase db, Customer obj);

        [CanBeNull]
        Customer Update(ChatDatabase db, uint id, Customer.UpdateInfo update);

        [NotNull]
        List<uint> GetIds(ChatDatabase db);


        [NotNull]
        List<uint> GetActiveIds(ChatDatabase db);
    }
}