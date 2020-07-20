using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.VisitorLeavesSession)]
    public class VisitorLeavesSessionChatEvent : ChatEventBase
    {
        public VisitorLeavesSessionChatEvent(
            DateTime timestampUtc,
            bool isDisconnected = false,
            bool isStopCalled = false)
            : base(timestampUtc, null)
        {
            IsDisconnected = isDisconnected;
            IsStopCalled = isStopCalled;
        }

        public VisitorLeavesSessionChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            IsDisconnected = AsBool(dbo.IS_DISCONNECTED);
            IsStopCalled = AsBool(dbo.IS_BECAME_OFFLINE);
        }

        public bool IsDisconnected { get; private set; }
        public bool IsStopCalled { get; private set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            session.IsVisitorConnected = false;

            var reason = "";
            if (IsDisconnected)
            {
                reason = IsStopCalled ? " because of navigation out of the page" : " because of the connection disconnect";
            }

            session.AddSystemMessage(this, false, "Посетитель покинул сессию {0}", reason);
        }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.IS_DISCONNECTED = AsSbyte(IsDisconnected);
            dbo.IS_BECAME_OFFLINE = AsSbyte(IsStopCalled);
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.VisitorLeftSession(chatSessionInfo, messages));
        }
    }
}