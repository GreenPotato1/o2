using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentRejectsAgentSession)]
    public class AgentRejectsAgentSessionChatEvent : ChatEventBase
    {
        public AgentRejectsAgentSessionChatEvent(
            DateTime timestampUtc,
            uint agentId)
            : base(timestampUtc, null)
        {
            AgentId = agentId;
        }

        public AgentRejectsAgentSessionChatEvent(CHAT_EVENT dbo)
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
                        "Session state: no pending invites for event {0}, agent {1}, in session {2}",
                        Id,
                        AgentId,
                        session.Skey));

            invite.Cancel(TimestampUtc, AgentId);

            if (!session.Invites.Any(x => x.IsPending) && session.Agents.Count == 1)
                session.Status = ChatSessionStatus.Completed;

            session.AddSystemMessage(this, invite.ActOnBehalfOfAgentId.HasValue, "Agent {0} rejected the session", agentName);
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

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.AgentSessionRejected(chatSessionInfo, agentInfo, messages));
        }
    }
}