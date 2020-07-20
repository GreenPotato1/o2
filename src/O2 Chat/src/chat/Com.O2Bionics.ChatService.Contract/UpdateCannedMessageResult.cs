using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class UpdateCannedMessageResult
    {
        public UpdateCannedMessageResult(CannedMessageInfo cannedMessage)
        {
            Status = new CallResultStatus(CallResultStatusCode.Success);
            CannedMessage = cannedMessage;
        }

        public UpdateCannedMessageResult(CallResultStatus status)
        {
            Status = status;
        }

        [DataMember]
        public CannedMessageInfo CannedMessage { get; set; }

        [DataMember]
        public CallResultStatus Status { get; set; }
    }
}