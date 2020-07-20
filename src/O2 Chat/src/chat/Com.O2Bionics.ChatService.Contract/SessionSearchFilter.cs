using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class SessionSearchFilter
    {
        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime EndDate { get; set; }

        [DataMember]
        public string SearchString { get; set; }

        [DataMember]
        public HashSet<decimal> Agents { get; set; }
    }
}