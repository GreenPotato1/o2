using System;
using System.Threading.Tasks;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.PageTracker.Contract
{
    public interface IPageTracker : IDisposable
    {
        [ItemNotNull]
        Task<AddRecordResult> Add(DateTime timestampUtc, [NotNull] AddRecordArgs args);

        [ItemNotNull]
        Task<GetHistoryResult> Get(uint customerId, ulong visitorId, int pageSize = PageTrackerConstants.PageHistoryGetLimit, [CanBeNull] SearchPositionInfo searchPosition = null);

        // Queue size is used for testing.
        int QueueSize { get; }

        void Flush();
    }
}