using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.VisitorStoppedMediaCall)]
    public class VisitorStoppedMediaCallChatEvent : ChatEventBase
    {
        public VisitorStoppedMediaCallChatEvent(DateTime timestampUtc, string text)
            : base(timestampUtc, text)
        {
        }

        public VisitorStoppedMediaCallChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
        }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            session.MediaCallStatus = MediaCallStatus.None;
            session.MediaCallAgentId = 0;
            session.MediaCallAgentHasVideo = null;
            session.MediaCallVisitorHasVideo = null;
            session.MediaCallAgentConnectionId = null;
            session.MediaCallVisitorConnectionId = null;

            session.AddSystemMessage(this, false, Text);
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var messages = chatSession.EventMessagesAsInfo(Id);
            var visitorVisibleMessages = messages.Where(x => !x.IsToAgentsOnly).ToList();

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.VisitorStoppedMediaCall(chatSessionInfo, messages));
            if (!chatSession.IsOffline && chatSession.VisitorId.HasValue)
                subscriptionManager.VisitorEventSubscribers.Publish(
                    x => x.VisitorStoppedMediaCall(chatSessionInfo, visitorVisibleMessages));
        }
    }
}