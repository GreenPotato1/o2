using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class ChatSessionAgentInfo
    {
        [DataMember]
        public uint AgentId { get; set; }

        [DataMember]
        public uint? ActsOnBehalfOfAgentId { get; set; }
    }
}