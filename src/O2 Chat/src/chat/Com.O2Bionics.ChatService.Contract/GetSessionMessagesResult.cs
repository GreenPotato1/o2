using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class GetSessionMessagesResult
    {
        [DataMember]
        public CallResultStatus Status { get; set; }

        [DataMember]
        public List<ChatSessionMessageInfo> Items { get; set; }

        [DataMember]
        public bool HasMore { get; set; }


        public GetSessionMessagesResult(int pageSize, List<ChatSessionMessageInfo> items)
        {
            Status = new CallResultStatus(CallResultStatusCode.Success);
            HasMore = items.Count > pageSize;
            Items = items.Take(pageSize).ToList();
        }

        public GetSessionMessagesResult(CallResultStatus status)
        {
            Status = status;
        }
    }
}