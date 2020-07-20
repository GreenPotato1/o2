using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentClosesSession)]
    public class AgentClosesSessionChatEvent : ChatEventBase
    {
        public AgentClosesSessionChatEvent(
            DateTime timestampUtc,
            string text,
            uint agentId)
            : base(timestampUtc, text)
        {
            AgentId = agentId;
        }

        public AgentClosesSessionChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
        }

        public uint AgentId { get; private set; }

        public override bool IsEndSessionEvent
        {
            get { return true; }
        }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            session.Status = ChatSessionStatus.Completed;

            session.Agents.Clear();

            var invites = session.Invites.Where(x => x.IsPending).ToList();
            foreach (var invite in invites)
                invite.Cancel(TimestampUtc, AgentId);

            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);
            session.AddSystemMessage(this, false, "Агент {0} закрыл сессию", agentName);

            session.MediaCallStatus = MediaCallStatus.None;
            session.MediaCallAgentHasVideo = null;
            session.MediaCallVisitorHasVideo = null;
            session.MediaCallAgentId = 0;
            session.MediaCallAgentConnectionId = null;
            session.MediaCallVisitorConnectionId = null;
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
                x => x.AgentClosedSession(chatSessionInfo, agentInfo, messages));
            if (!chatSession.IsOffline && chatSession.VisitorId.HasValue)
                subscriptionManager.VisitorEventSubscribers.Publish(
                    x => x.AgentClosedSession(chatSessionInfo, agentInfo, visitorVisibleMessages));
        }
    }
}