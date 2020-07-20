using System;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class CannedMessageInfo
    {
        [DataMember]
        public uint Id { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public uint? UserId { get; set; }

        [DataMember]
        public uint? DepartmentId { get; set; }

        [DataMember]
        public DateTime AddTimestampUtc { get; set; }

        [DataMember]
        public DateTime UpdateTimestampUtc { get; set; }
    }
}