using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentInvitesAgent)]
    public class AgentInvitesAgentChatEvent : ChatEventBase
    {
        public AgentInvitesAgentChatEvent(
            DateTime timestampUtc,
            string text,
            uint agentId,
            uint invitedAgentId,
            bool actOnBehalfOfInvitor)
            : base(timestampUtc, text)
        {
            AgentId = agentId;
            InvitedAgentId = invitedAgentId;
            ActOnBehalfOfInvitor = actOnBehalfOfInvitor;
        }

        public AgentInvitesAgentChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
            InvitedAgentId = dbo.TARGET_AGENT_ID.Value;
            ActOnBehalfOfInvitor = AsBool(dbo.ACT_ON_BEHALF_OF_INVITOR);
        }

        public uint AgentId { get; private set; }
        public uint InvitedAgentId { get; private set; }
        public bool ActOnBehalfOfInvitor { get; private set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            session.Status = ChatSessionStatus.Active;

            if (session.Agents.Any(x => x.AgentId == InvitedAgentId))
                return;
            if (session.Invites.OfType<ChatSessionAgentInvite>().Any(x => x.IsPending && x.AgentId == InvitedAgentId))
                return;

            session.Invites.Add(
                new ChatSessionAgentInvite(TimestampUtc, AgentId, InvitedAgentId, ActOnBehalfOfInvitor ? AgentId : (uint?)null));

            session.AgentsInvolved.Add(InvitedAgentId);
            session.DepartmentsInvolved.UnionWith(resolver.GetAgentDepartments(session.CustomerId, InvitedAgentId));

            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);
            var invitedAgentName = resolver.GetAgentName(session.CustomerId, InvitedAgentId);
            session.AddSystemMessage(this, ActOnBehalfOfInvitor, "Агент {0} пригласил {1} к сессии", agentName, invitedAgentName);
        }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
            dbo.TARGET_AGENT_ID = InvitedAgentId;
            dbo.ACT_ON_BEHALF_OF_INVITOR = AsSbyte(ActOnBehalfOfInvitor);
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var agentInfo = resolver.GetAgentInfo(chatSession.CustomerId, AgentId);
            var invitedAgentInfo = resolver.GetAgentInfo(chatSession.CustomerId, InvitedAgentId);
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.AgentInvited(chatSessionInfo, agentInfo, invitedAgentInfo, messages));
        }
    }
}