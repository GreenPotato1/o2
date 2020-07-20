using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class UserLoginResult
    {
        [DataMember]
        public AccountLookupStatus Status { get; set; }

        [DataMember]
        [CanBeNull]
        public UserInfo User { get; set; }
    }
}