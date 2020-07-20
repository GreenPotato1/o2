using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.VisitorReconnect)]
    public class VisitorReconnectChatEvent : ChatEventBase
    {
        public VisitorReconnectChatEvent(DateTime timestampUtc)
            : base(timestampUtc, null)
        {
        }

        public VisitorReconnectChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
        }


        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            session.IsVisitorConnected = true;

            session.AddSystemMessage(this, false, "Посетитель был повторно подключен к сеансу");
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.VisitorReconnected(chatSessionInfo, messages));
        }
    }
}