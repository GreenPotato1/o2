using System;
using System.Net;

namespace Com.O2Bionics.PageTracker.Contract
{
    public sealed class PageHistoryVisitorInfo
    {
        public DateTime TimestampUtc { get; set; }
        public string VisitorExternalId { get; set; }
        public IPAddress Ip { get; set; }
        public GeoLocation IpLocation { get; set; }
        public TimeZoneDescription TimeZone { get; set; }
        public UserAgentInfo UserAgent { get; set; }
    }
}