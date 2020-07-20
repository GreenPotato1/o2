using System;
using System.Runtime.Serialization;
using Jil;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class ChatSessionMessageInfo
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public long EventId { get; set; }

        [DataMember]
        public DateTime TimestampUtc { get; set; }

        [DataMember]
        [JilDirective(TreatEnumerationAs = typeof(int))]
        public ChatMessageSender Sender { get; set; }

        [DataMember]
        public string SenderAgentName { get; set; }

        [DataMember]
        public uint? SenderAgentId { get; set; }

        [DataMember]
        public string OnBehalfOfName { get; set; }

        [DataMember]
        public uint? OnBehalfOfId { get; set; }

        [DataMember]
        public bool IsToAgentsOnly { get; set; }

        [DataMember]
        public string Text { get; set; }
    }
}