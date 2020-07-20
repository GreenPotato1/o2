using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Jil;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    [DebuggerDisplay("{Id} '{Name}' {Status} {null == Domains || 0 == Domains.Count ? null : Domains[0]}")]
    public sealed class CustomerInfo
    {
        //If you change this class, make changes to the "SpecificClassDiff" class.

        [DataMember]
        public uint Id { get; set; }

        [DataMember]
        public DateTime AddTimestampUtc { get; set; }

        [DataMember]
        public DateTime UpdateTimestampUtc { get; set; }

        [DataMember]
        [JilDirective(TreatEnumerationAs = typeof(int))]
        public ObjectStatus Status { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string[] Domains { get; set; }

        [DataMember]
        public string CreateIp { get; set; }
    }
}