using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentAcceptsAgentSession)]
    public class AgentAcceptsAgentSessionChatEvent : ChatEventBase
    {
        public AgentAcceptsAgentSessionChatEvent(
            DateTime timestampUtc,
            uint agentId)
            : base(timestampUtc, null)
        {
            AgentId = agentId;
        }

        public AgentAcceptsAgentSessionChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
        }

        public uint AgentId { get; private set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);

            var invite = session.Invites
                .OfType<ChatSessionAgentInvite>()
                .FirstOrDefault(x => x.IsPending && x.AgentId == AgentId);
            if (invite == null)
                throw new InvalidOperationException(
                    string.Format(
                        "Session state: no pending agent invites for event {0}, agent {1} in session {2}",
                        Id,
                        AgentId,
                        session.Skey));

            session.Status = ChatSessionStatus.Active;

            invite.Accept(TimestampUtc, AgentId);
            session.Agents.Add(new ChatSessionAgent(AgentId, invite.ActOnBehalfOfAgentId));

            session.AddSystemMessage(this, false, "Агент {0} принял сессию", agentName);
        }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var agentInfo = resolver.GetAgentInfo(chatSession.CustomerId, AgentId);
            var messages = chatSession.EventMessagesAsInfo(Id);
            var visitorVisibleMessages = messages.Where(x => !x.IsToAgentsOnly).ToList();

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.AgentSessionAccepted(chatSessionInfo, agentInfo, messages));
            if (!chatSession.IsOffline && chatSession.VisitorId.HasValue)
                subscriptionManager.VisitorEventSubscribers.Publish(
                    x => x.AgentSessionAccepted(chatSessionInfo, agentInfo, visitorVisibleMessages));
        }
    }
}