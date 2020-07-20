using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class GetCannedMessagesResult
    {
        public GetCannedMessagesResult(List<CannedMessageInfo> cannedMessages)
        {
            Status = new CallResultStatus(CallResultStatusCode.Success);
            CannedMessages = cannedMessages;
        }

        public GetCannedMessagesResult(CallResultStatus status)
        {
            Status = status;
        }

        [DataMember]
        public List<CannedMessageInfo> CannedMessages { get; set; }

        [DataMember]
        public CallResultStatus Status { get; set; }
    }
}