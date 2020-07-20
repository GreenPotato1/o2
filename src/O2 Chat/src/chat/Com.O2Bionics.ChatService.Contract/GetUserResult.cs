using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class GetUserResult
    {
        [DataMember]
        public UserInfo UserInfo { get; set; }

        [DataMember]
        public bool AreAvatarsAllowed { get; set; }
    }
}