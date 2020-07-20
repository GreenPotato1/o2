using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.VisitorSetMediaCallConnectionId)]
    public class VisitorSetsMediaCallConnectionIdChatEvent : ChatEventBase
    {
        public VisitorSetsMediaCallConnectionIdChatEvent(DateTime timestampUtc, string connectionId)
            : base(timestampUtc, null)
        {
            ConnectionId = connectionId;
        }

        public VisitorSetsMediaCallConnectionIdChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
        }

        private string ConnectionId { get; set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            session.MediaCallVisitorConnectionId = ConnectionId;
            session.MediaCallStatus = MediaCallStatus.Established;
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var sessionInfo = chatSession.AsInfo();
            subscriptionManager.AgentEventSubscribers.Publish(
                s => s.MediaCallVisitorConnectionIdSet(sessionInfo));
        }
    }
}