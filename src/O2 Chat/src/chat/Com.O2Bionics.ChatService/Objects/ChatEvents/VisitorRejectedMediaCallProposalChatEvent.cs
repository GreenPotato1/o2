using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.VisitorRejectedMediaCallProposal)]
    public class VisitorRejectedMediaCallProposalChatEvent : ChatEventBase
    {
        public VisitorRejectedMediaCallProposalChatEvent(DateTime timestampUtc)
            : base(timestampUtc, null)
        {
        }

        public VisitorRejectedMediaCallProposalChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
        }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            session.AddSystemMessage(this, false, "Visitor has been rejected media call proposal");

            session.MediaCallStatus = MediaCallStatus.None;
            session.MediaCallAgentId = 0;
            session.MediaCallAgentHasVideo = null;
            session.MediaCallVisitorHasVideo = null;
            session.MediaCallAgentConnectionId = null;
            session.MediaCallVisitorConnectionId = null;
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.VisitorRejectedMediaCallProposal(chatSessionInfo, messages));
            if (!chatSession.IsOffline && chatSession.VisitorId.HasValue)
                subscriptionManager.VisitorEventSubscribers.Publish(
                    x => x.VisitorRejectedMediaCallProposal(chatSession.VisitorId.Value));
        }
    }
}