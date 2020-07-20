using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class AgentSessionConnectResult
    {
        [DataMember]
        public uint AgentId { get; set; }

        [DataMember]
        public uint CustomerId { get; set; }

        [DataMember]
        public HashSet<uint> Departments { get; set; }
    }
}