using System.Diagnostics;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    [DebuggerDisplay("{Email}, {ClientAddress}, {ClientType}, {ClientVersion}")]
    public class LoginParameters
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string ClientType { get; set; }

        [DataMember]
        public string ClientVersion { get; set; }

        [DataMember]
        public string ClientAddress { get; set; }

        [DataMember]
        public string ClientLocalDate { get; set; }
    }
}