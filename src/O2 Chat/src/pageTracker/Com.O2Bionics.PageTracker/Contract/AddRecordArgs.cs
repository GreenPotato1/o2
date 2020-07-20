using System;
using System.Net;

namespace Com.O2Bionics.PageTracker.Contract
{
    public sealed class AddRecordArgs
    {
        public uint CustomerId { get; set; }
        public string VisitorExternalId { get; set; }
        public ulong VisitorId { get; set; }
        public IPAddress Ip { get; set; }
        public TimeZoneDescription TimeZone { get; set; }
        public string UserAgentString { get; set; }
        public Uri Url { get; set; }
        public string CustomText { get; set; }
    }
}