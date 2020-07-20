using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class ResetPasswordResult
    {
        [DataMember]
        public ResetPasswordCodeStatus Status { get; set; }

        [DataMember]
        public string Email { get; set; }
    }
}