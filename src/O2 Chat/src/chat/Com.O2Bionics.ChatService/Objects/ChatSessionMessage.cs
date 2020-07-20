using System;
using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Objects
{
    public class ChatSessionMessage
    {
        public ChatSessionMessage(
            int id,
            long eventId,
            DateTime timestampUtc,
            ChatMessageSender sender,
            uint? senderAgentId,
            string senderAgentName,
            uint? onBehalfOfId,
            string onBehalfOfName,
            bool isToAgentsOnly,
            string text)
        {
            Id = id;
            EventId = eventId;
            TimestampUtc = timestampUtc;
            Sender = sender;
            SenderAgentId = senderAgentId;
            SenderAgentName = senderAgentName;
            OnBehalfOfName = onBehalfOfName;
            OnBehalfOfId = onBehalfOfId;
            IsToAgentsOnly = isToAgentsOnly;
            Text = text;
        }

        public int Id { get; private set; }
        public long EventId { get; private set; }
        public DateTime TimestampUtc { get; private set; }
        public ChatMessageSender Sender { get; private set; }
        public uint? SenderAgentId { get; private set; }
        public string SenderAgentName { get; private set; }
        public string OnBehalfOfName { get; private set; }
        public uint? OnBehalfOfId { get; private set; }
        public bool IsToAgentsOnly { get; private set; }
        public string Text { get; private set; }

        public ChatSessionMessageInfo AsInfo()
        {
            return new ChatSessionMessageInfo
                {
                    Id = Id,
                    EventId = EventId,
                    TimestampUtc = TimestampUtc,
                    Sender = Sender,
                    SenderAgentName = SenderAgentName,
                    SenderAgentId = SenderAgentId,
                    OnBehalfOfName = OnBehalfOfName,
                    OnBehalfOfId = OnBehalfOfId,
                    IsToAgentsOnly = IsToAgentsOnly,
                    Text = Text,
                };
        }
    }
}