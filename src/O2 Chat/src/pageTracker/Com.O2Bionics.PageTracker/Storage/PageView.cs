using System;
using System.Runtime.Serialization;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.Utils;
using Nest;
using GeoLocation = Com.O2Bionics.PageTracker.Contract.GeoLocation;

namespace Com.O2Bionics.PageTracker.Storage
{
    [DataContract]
    [ElasticsearchType(Name = FieldConstants.PreferredTypeName, IdProperty = nameof(Id))]
    public sealed class PageView
    {
        [DataMember]
        [Number(NumberType.Long)]
        public string Id { get; set; }

        [DataMember(IsRequired = true)]
        [Date]
        public DateTime Timestamp { get; set; }

        [DataMember(IsRequired = true)]
        [Number(NumberType.Integer)]
        public uint CustomerId { get; set; }

        [DataMember(IsRequired = true)]
        [Number(NumberType.Long)]
        public long VisitorId { get; set; }

        [DataMember]
        [Keyword(IgnoreAbove = PageTrackerConstants.VisitorExternalIdMaxLength)]
        public string VisitorExternalId { get; set; }

        [DataMember]
        public UriInfo UriInfo { get; set; }

        #region IP address

        [DataMember]
        [Ip]
        public string IpAddress { get; set; }

        [DataMember]
        public GeoLocation Location { get; set; }

        [IgnoreDataMember]
        [Ignore]
        public bool IsResolved => null != Location?.Country;

        #endregion

        [DataMember]
        public UserAgentInfo UserAgent { get; set; }

        [DataMember]
        public TimeZoneDescription TimeZone { get; set; }

        [DataMember]
        [Text]
        public string CustomText { get; set; }
    }
}