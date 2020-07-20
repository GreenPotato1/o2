using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class SessionSearchResult
    {
        [DataMember]
        public CallResultStatus Status { get; set; }

        [DataMember]
        public List<ChatSessionInfo> Items { get; set; }

        [DataMember]
        public bool HasMore { get; set; }

        [DataMember]
        public List<VisitorInfo> Visitors { get; set; }


        public SessionSearchResult(CallResultStatus status)
        {
            Status = status;
        }

        public SessionSearchResult(int pageSize, List<ChatSessionInfo> items, List<VisitorInfo> visitors)
        {
            Status = new CallResultStatus(CallResultStatusCode.Success);
            HasMore = items.Count > pageSize;
            Items = items.Take(pageSize).ToList();
            Visitors = visitors;
        }
    }
}