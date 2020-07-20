using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentCancelsInviteAgent)]
    public class AgentCancelsInviteAgentChatEvent : ChatEventBase
    {
        public AgentCancelsInviteAgentChatEvent(
            DateTime timestampUtc,
            string text,
            uint agentId,
            uint invitedAgentId)
            : base(timestampUtc, text)
        {
            AgentId = agentId;
            InvitedAgentId = invitedAgentId;
        }

        public AgentCancelsInviteAgentChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
            InvitedAgentId = dbo.TARGET_AGENT_ID.Value;
        }

        public uint AgentId { get; private set; }
        public uint InvitedAgentId { get; private set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            var invite = session.Invites
                .OfType<ChatSessionAgentInvite>()
                .FirstOrDefault(x => x.IsPending && x.AgentId == InvitedAgentId);
            if (invite == null) return;

            invite.Cancel(TimestampUtc, AgentId);

            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);
            var invitedAgentName = resolver.GetAgentName(session.CustomerId, InvitedAgentId);
            session.AddSystemMessage(
                this,
                invite.ActOnBehalfOfAgentId.HasValue,
                "Agent {0} has been canceled the invitation for {1}",
                agentName,
                invitedAgentName);
        }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
            dbo.TARGET_AGENT_ID = InvitedAgentId;
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var agentInfo = resolver.GetAgentInfo(chatSession.CustomerId, AgentId);
            var invitedAgentInfo = resolver.GetAgentInfo(chatSession.CustomerId, InvitedAgentId);
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.AgentInvitationCanceled(chatSessionInfo, agentInfo, invitedAgentInfo, messages));
        }
    }
}