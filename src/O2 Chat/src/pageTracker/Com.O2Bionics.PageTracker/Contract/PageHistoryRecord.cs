using System;

namespace Com.O2Bionics.PageTracker.Contract
{
    public sealed class PageHistoryRecord
    {
        public string Id { get; set; }
        public DateTime TimestampUtc { get; set; }
        public Uri Url { get; set; }
        public string CustomText { get; set; }

        public override string ToString()
        {
            return $"Id={Id}, Time={TimestampUtc}, Url={Url}, Text={CustomText}";
        }
    }
}