using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentStoppedMediaCall)]
    public class AgentStoppedMediaCallChatEvent : ChatEventBase
    {
        public AgentStoppedMediaCallChatEvent(
            DateTime timestampUtc,
            string text,
            uint agentId)
            : base(timestampUtc, text)
        {
            AgentId = agentId;
        }

        public AgentStoppedMediaCallChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID ?? 0;
        }

        public uint AgentId { get; private set; }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
        }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            var agent = session.Agents.FirstOrDefault(x => x.AgentId == AgentId);
            if (agent == null)
                throw new InvalidOperationException(string.Format("Agent {0} is not participating in the session {1}", AgentId, session.Skey));

            session.MediaCallStatus = MediaCallStatus.None;
            session.MediaCallAgentId = 0;
            session.MediaCallAgentHasVideo = null;
            session.MediaCallVisitorHasVideo = null;
            session.MediaCallAgentConnectionId = null;
            session.MediaCallVisitorConnectionId = null;

            session.AddSystemMessage(this, false, Text);
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var agentInfo = resolver.GetAgentInfo(chatSession.CustomerId, AgentId);
            var messages = chatSession.EventMessagesAsInfo(Id);
            var visitorVisibleMessages = messages.Where(x => !x.IsToAgentsOnly).ToList();

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.AgentStoppedMediaCall(chatSessionInfo, agentInfo, messages));
            if (!chatSession.IsOffline && chatSession.VisitorId.HasValue)
                subscriptionManager.VisitorEventSubscribers.Publish(
                    x => x.AgentStoppedMediaCall(chatSessionInfo, agentInfo, visitorVisibleMessages));
        }
    }
}