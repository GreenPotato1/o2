using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using Nest;

namespace Com.O2Bionics.ErrorTracker
{
    /// <summary>
    /// An error, that has occurred in the client JS application.
    /// Mark the members, coming from the JS client, with <see cref="DataMember"/>.
    /// </summary>
    [DataContract]
    [ElasticsearchType(Name = FieldConstants.PreferredTypeName)]
    [DebuggerDisplay(
        "c={CustomerId}, v={VisitorId}, u={UserId}, {Timestamp}, {Application} {Url}, {Message}, {ExceptionMessage} {ExceptionStack}")]
    public sealed class ErrorInfo
    {
        #region Application

        [IgnoreDataMember] internal const string ApplicationPropertyName = "Application";

        [Keyword(Name = ApplicationPropertyName)]
        [DataMember]
        public string Application { get; set; }

        [DataMember(Name = ServiceConstants.CustomerId, IsRequired = false)]
        [Number(NumberType.Long, Name = ServiceConstants.CustomerId)]
        public uint CustomerId { get; set; }

        /// <summary>
        /// Only for Widget.
        /// </summary>
        [DataMember(Name = ServiceConstants.VisitorId, IsRequired = false)]
        [Number(NumberType.Long, Name = ServiceConstants.VisitorId)]
        public ulong VisitorId { get; set; }

        /// <summary>
        /// Only for Workspace.
        /// </summary>
        [DataMember(Name = ServiceConstants.UserId, IsRequired = false)]
        [Number(NumberType.Long, Name = ServiceConstants.UserId)]
        public uint UserId { get; set; }

        #endregion


        #region Exception

        [DataMember(Name = "Message", IsRequired = false)]
        //[Text(Name = "Message")]
        [LongString]
        public string Message { get; set; }

        [DataMember(Name = "ExceptionMessage", IsRequired = false)]
        //[Text(Name = "ExceptionMessage")]
        [LongString]
        public string ExceptionMessage { get; set; }

        [DataMember(Name = "ExceptionStack", IsRequired = false)]
        //[Text(Name = "ExceptionStack")]
        [LongString]
        public string ExceptionStack { get; set; }

        [DataMember]
        //[Text(Name = "ExceptionSource")]
        [LongString]
        public string ExceptionSource { get; set; }

        [DataMember]
        //[Text(Name = "ExceptionType")]
        [LongString]
        public string ExceptionType { get; set; }

        [DataMember]
        //[Text(Name = "LoggerName")]
        [LongString]
        public string LoggerName { get; set; }

        [DataMember(Name = "Url", IsRequired = false)]
        //[Text(Name = "Url")]
        [LongString]
        public string Url { get; set; }

        #endregion

        [IgnoreDataMember] internal const string TimeZoneOffsetPropertyName = "TimeZoneOffset";

        [DataMember(Name = TimeZoneOffsetPropertyName, IsRequired = false)]
        [Number(NumberType.Integer, Name = TimeZoneOffsetPropertyName)]
        public int TimeZoneOffset { get; set; }

        [IgnoreDataMember] internal const string TimeZonePropertyName = "TimeZoneName";

        [DataMember(Name = TimeZonePropertyName, IsRequired = false)]
        //[Text(Name = TimeZonePropertyName)]
        [LongString]
        public string TimeZoneName { get; set; }

        [DataMember]
        //[Text(Name = "HostName")]
        [LongString]
        public string HostName { get; set; }

        [DataMember]
        //[Text(Name = "ClientIp")]
        public string ClientIp { get; set; }

        [DataMember]
        //[Text(Name = "UserAgent")]
        [LongString]
        public string UserAgent { get; set; }

        [IgnoreDataMember] internal const string TimestampPropertyName = "Timestamp";

        [DataMember]
        [Date(Name = TimestampPropertyName)]
        public DateTime Timestamp { get; set; }
    }
}