using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class UpdateUserResult
    {
        public UpdateUserResult()
        {
        }

        public UpdateUserResult(CallResultStatus status, UserInfo user = null)
        {
            Status = status;
            User = user;
        }

        [DataMember]
        public CallResultStatus Status { get; set; }

        [DataMember]
        public UserInfo User { get; set; }
    }
}