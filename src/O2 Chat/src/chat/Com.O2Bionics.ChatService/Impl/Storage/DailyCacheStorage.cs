using System;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public abstract class DailyCacheStorage<T> : DailyCache<T>, INotifyableStorage
    {
        protected readonly INowProvider NowProvider;
        protected readonly IAuditTrailClient AuditTrailClient;

        [CanBeNull]
        protected ICustomerCacheNotifier CustomerCache { get; private set; }

        protected DailyCacheStorage([NotNull] INowProvider nowProvider, [NotNull] IAuditTrailClient auditTrailClient)
        {
            NowProvider = nowProvider ?? throw new ArgumentNullException(nameof(nowProvider));
            AuditTrailClient = auditTrailClient ?? throw new ArgumentNullException(nameof(auditTrailClient));
        }

        public void SetNotifier(ICustomerCacheNotifier customerCacheNotifier)
        {
            CustomerCache = customerCacheNotifier ?? throw new ArgumentNullException(nameof(customerCacheNotifier));
        }

        public sealed override async Task Load()
        {
            var now = NowProvider.UtcNow;
            var date = now.RemoveTime();
            var days = date.ToDays();
            var dailyInfo = new DailyInfo<T>(days);

            await LoadSetValues(dailyInfo, date);
            Set(dailyInfo);
            if (Log.IsDebugEnabled)
                Log.Debug($"{nameof(Load)} took {(NowProvider.UtcNow - now).TotalMilliseconds} ms.");
#if DEBUG
            IsReady = true;
#endif
        }

        protected abstract Task LoadSetValues([NotNull] DailyInfo<T> dailyInfo, DateTime date);
    }
}