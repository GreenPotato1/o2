using Com.O2Bionics.ChatService.Contract;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService
{
    public interface INotifyableStorage
    {
        void SetNotifier([NotNull] ICustomerCacheNotifier customerCacheNotifier);
    }
}