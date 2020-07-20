using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.VisitorSendsMessage)]
    public class VisitorSendsMessageChatEvent : ChatEventBase
    {
        public VisitorSendsMessageChatEvent(
            DateTime timestampUtc,
            string text)
            : base(timestampUtc, text)
        {
        }

        public VisitorSendsMessageChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
        }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            session.AddVisitorMessage(this);
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.VisitorEventSubscribers.Publish(
                x => x.VisitorMessage(chatSessionInfo.VisitorId.Value, messages));
            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.VisitorMessage(chatSessionInfo, messages));
        }
    }
}