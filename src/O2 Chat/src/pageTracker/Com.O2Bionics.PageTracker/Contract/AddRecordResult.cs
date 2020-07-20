using Newtonsoft.Json;

namespace Com.O2Bionics.PageTracker.Contract
{
    public sealed class AddRecordResult
    {
        [JsonProperty(PropertyName = PageTrackerConstants.VisitorIdParameterName)]
        public ulong VisitorId { get; set; }

        [JsonProperty(PropertyName = PageTrackerConstants.HistoryIdParameterName)]
        public string PageHistoryId { get; set; }

        public override string ToString()
        {
            return $"{nameof(VisitorId)}={VisitorId}, {nameof(PageHistoryId)}={PageHistoryId}";
        }
    }
}