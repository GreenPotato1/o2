using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.SessionTranscriptSentToVisitor)]
    public class SessionTranscriptSentToVisitorChatEvent : ChatEventBase
    {
        public SessionTranscriptSentToVisitorChatEvent(
            DateTime timestampUtc,
            uint agentId,
            long lastSentEventId)
            : base(timestampUtc, null)
        {
            AgentId = agentId;
            LastSentEventId = lastSentEventId;
        }

        public SessionTranscriptSentToVisitorChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
            LastSentEventId = Int32.Parse(dbo.TEXT);
        }

        public uint AgentId { get; private set; }
        public long LastSentEventId { get; private set; }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
            dbo.TEXT = LastSentEventId.ToString("D");
        }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            session.VisitorTranscriptLastEvent = LastSentEventId;
            session.VisitorTranscriptTimestampUtc = TimestampUtc;

            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);
            session.AddSystemMessage(this, false, "Агент {0} отправил транскрипт сессии пользователю", agentName);
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.SessionTranscriptSentToVisitor(chatSessionInfo, messages));
        }
    }
}