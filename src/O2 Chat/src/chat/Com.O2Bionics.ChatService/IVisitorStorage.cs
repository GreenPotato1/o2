using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.ChatService.Objects;

namespace Com.O2Bionics.ChatService
{
    public interface IVisitorStorage : IDbUpdaterStorage<Visitor>
    {
        Visitor Get(ulong uniqueId);
        Visitor GetOrCreate(uint customerId, ulong id);

        VisitorUpdate Update(ulong id, VisitorUpdate update);
    }
}