using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class AgentConsoleSettingsInfo
    {
        [DataMember]
        public int MediaCallProposalTimeoutMs { get; set; }

        [DataMember]
        public int MediaCallConnectTimeoutMs { get; set; }
    }
}