using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.VisitorAcceptedMediaCallProposal)]
    public class VisitorAcceptedMediaCallProposalChatEvent : ChatEventBase
    {
        public VisitorAcceptedMediaCallProposalChatEvent(
            DateTime timestampUtc,
            bool hasVideo)
            : base(timestampUtc, null)
        {
            HasVideo = hasVideo;
        }

        public VisitorAcceptedMediaCallProposalChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            HasVideo = AsBool(dbo.HAS_VIDEO);
        }

        public bool HasVideo { get; private set; }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.HAS_VIDEO = AsSbyte(HasVideo);
        }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            var mediaType = HasVideo ? "video" : "voice";
            session.AddSystemMessage(this, false, "Visitor has been accepted media call proposal with {0}", mediaType);

            session.MediaCallStatus = MediaCallStatus.AcceptedByVisitor;
            session.MediaCallVisitorHasVideo = HasVideo;
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.VisitorAcceptedMediaCallProposal(chatSessionInfo, messages));
            if (!chatSession.IsOffline && chatSession.VisitorId.HasValue)
                subscriptionManager.VisitorEventSubscribers.Publish(
                    x => x.VisitorAcceptedMediaCallProposal(chatSession.VisitorId.Value));
        }
    }
}