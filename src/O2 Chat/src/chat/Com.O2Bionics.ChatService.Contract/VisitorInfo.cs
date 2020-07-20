using System;
using System.Runtime.Serialization;
using Jil;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class VisitorInfo
    {
        [DataMember]
        public ulong UniqueId { get; set; }

        [DataMember]
        public DateTime AddTimestampUtc { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Phone { get; set; }

        [DataMember]
        public MediaSupport MediaSupport { get; set; }

        [DataMember]
        [JilDirective(TreatEnumerationAs = typeof(int))]
        public VisitorSendTranscriptMode? TranscriptMode { get; set; }
    }
}