using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class FullChatSessionInfo : ChatSessionInfo
    {
        [DataMember]
        public List<ChatSessionMessageInfo> Messages { get; set; }
    }
}