using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentSendsMessage)]
    public class AgentSendsMessageChatEvent : ChatEventBase
    {
        public AgentSendsMessageChatEvent(
            DateTime timestampUtc,
            string text,
            uint agentId,
            bool isToAgentsOnly)
            : base(timestampUtc, text)
        {
            AgentId = agentId;
            IsToAgentsOnly = isToAgentsOnly;
        }

        public AgentSendsMessageChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
            IsToAgentsOnly = AsBool(dbo.IS_TO_AGENTS_ONLY);
        }

        public uint AgentId { get; private set; }
        public bool IsToAgentsOnly { get; private set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);

            var agent = session.Agents.FirstOrDefault(x => x.AgentId == AgentId);
            if (agent == null)
                throw new InvalidOperationException(string.Format("Agent {0} is not participating in the session {1}", AgentId, session.Skey));

            var onBehalfOfName = agent.ActsOnBehalfOfAgentId.HasValue
                ? resolver.GetAgentName(session.CustomerId, agent.ActsOnBehalfOfAgentId.Value)
                : null;

            session.AddAgentMessage(this, AgentId, agentName, IsToAgentsOnly, agent.ActsOnBehalfOfAgentId, onBehalfOfName);
        }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
            dbo.IS_TO_AGENTS_ONLY = AsSbyte(IsToAgentsOnly);
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var agentInfo = resolver.GetAgentInfo(chatSession.CustomerId, AgentId);
            var messages = chatSession.EventMessagesAsInfo(Id);
            var visitorVisibleMessages = messages.Where(x => !x.IsToAgentsOnly).ToList();

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.AgentMessage(chatSessionInfo, agentInfo, messages));
            if (!chatSession.IsOffline && chatSession.VisitorId.HasValue)
                subscriptionManager.VisitorEventSubscribers.Publish(
                    x => x.AgentMessage(chatSessionInfo, agentInfo, visitorVisibleMessages));
        }
    }
}