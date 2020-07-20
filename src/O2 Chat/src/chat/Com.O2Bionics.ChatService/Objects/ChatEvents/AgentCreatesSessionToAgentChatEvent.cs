using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentCreatesSessionToAgent)]
    public class AgentCreatesSessionToAgentChatEvent : AgentCreatesSessionChatEventBase
    {
        public AgentCreatesSessionToAgentChatEvent(
            DateTime timestampUtc,
            string text,
            uint agentId,
            uint targetAgentId)
            : base(timestampUtc, text)
        {
            AgentId = agentId;
            TargetAgentId = targetAgentId;
        }

        public AgentCreatesSessionToAgentChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
            TargetAgentId = dbo.TARGET_AGENT_ID.Value;
        }

        public uint AgentId { get; private set; }
        public uint TargetAgentId { get; private set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            if (session.IsOffline)
                throw new InvalidOperationException("Агент не может начать автономный сеанс с агентом");

            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);
            var targetAgentName = resolver.GetAgentName(session.CustomerId, TargetAgentId);

            session.Status = ChatSessionStatus.Queued;

            session.Invites.Add(
                new ChatSessionAgentInvite(TimestampUtc, AgentId, TargetAgentId));
            session.Agents.Add(new ChatSessionAgent(AgentId));

            session.AgentsInvolved.Add(AgentId);
            session.AgentsInvolved.Add(TargetAgentId);
            session.DepartmentsInvolved.UnionWith(resolver.GetAgentDepartments(session.CustomerId, AgentId));

            session.AddSystemMessage(this, false, "Сессия с агентом {0} создана", targetAgentName);
            session.AddAgentMessage(this, AgentId, agentName);
        }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
            dbo.TARGET_AGENT_ID = TargetAgentId;
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.AgentSessionCreated(chatSessionInfo, messages));
        }
    }
}