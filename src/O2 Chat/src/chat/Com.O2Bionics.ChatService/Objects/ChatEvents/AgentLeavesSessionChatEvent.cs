using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentLeavesSession)]
    public class AgentLeavesSessionChatEvent : ChatEventBase
    {
        public AgentLeavesSessionChatEvent(
            DateTime timestampUtc,
            string text,
            uint agentId,
            bool isDisconnected,
            bool isBecameOffline)
            : base(timestampUtc, text)
        {
            AgentId = agentId;
            IsDisconnected = isDisconnected;
            IsBecameOffline = isBecameOffline;
        }

        public AgentLeavesSessionChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
            IsDisconnected = AsBool(dbo.IS_DISCONNECTED);
            IsBecameOffline = AsBool(dbo.IS_BECAME_OFFLINE);
        }

        public uint AgentId { get; private set; }
        public bool IsDisconnected { get; private set; }
        public bool IsBecameOffline { get; private set; }

        private bool WasAgentVisibleToVisitor { get; set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            var agent = session.Agents.FirstOrDefault(x => x.AgentId == AgentId);
            if (agent == null) return;

            WasAgentVisibleToVisitor = !agent.ActsOnBehalfOfAgentId.HasValue;

            session.Agents.Remove(agent);

            var hasNoAgentsVisibleToVisitor =
                session.Agents.All(x => x.ActsOnBehalfOfAgentId != null);

            if (hasNoAgentsVisibleToVisitor && session.Invites.All(x => !x.IsPending))
            {
                session.Status = ChatSessionStatus.Queued;

                if (session.Invites.Count > 0)
                    session.Invites.Add(session.Invites[0].CreatePendingClone(TimestampUtc));
            }

            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);
            session.AddSystemMessage(this, false, "Агент {0} покинул сессию", agentName);
        }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
            dbo.IS_DISCONNECTED = AsSbyte(IsDisconnected);
            dbo.IS_BECAME_OFFLINE = AsSbyte(IsBecameOffline);
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var agentInfo = resolver.GetAgentInfo(chatSession.CustomerId, AgentId);
            var messages = chatSession.EventMessagesAsInfo(Id);
            var visitorVisibleMessages = messages.Where(x => !x.IsToAgentsOnly).ToList();

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.AgentLeftSession(chatSessionInfo, agentInfo, messages));
            if (!chatSession.IsOffline && chatSession.VisitorId.HasValue && WasAgentVisibleToVisitor)
                subscriptionManager.VisitorEventSubscribers.Publish(
                    x => x.AgentLeftSession(chatSessionInfo, agentInfo, visitorVisibleMessages));
        }
    }
}