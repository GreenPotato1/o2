using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class UserAgentInfo
    {
        [DataMember]
        public string Device { get; set; }

        [DataMember]
        public string Os { get; set; }

        [DataMember]
        public string UserAgent { get; set; }
    }
}