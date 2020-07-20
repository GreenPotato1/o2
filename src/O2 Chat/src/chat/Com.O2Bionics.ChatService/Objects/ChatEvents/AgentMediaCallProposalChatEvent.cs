using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentMediaCallProposal)]
    public class AgentMediaCallProposalChatEvent : ChatEventBase
    {
        public AgentMediaCallProposalChatEvent(
            DateTime timestampUtc,
            uint agentId,
            bool hasVideo,
            string agentConnectionId)
            : base(timestampUtc, null)
        {
            AgentId = agentId;
            HasVideo = hasVideo;
            AgentConnectionId = agentConnectionId;
        }

        public AgentMediaCallProposalChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
            HasVideo = AsBool(dbo.HAS_VIDEO);
        }

        public uint AgentId { get; private set; }
        public bool HasVideo { get; private set; }

        // not persistent
        public string AgentConnectionId { get; private set; }


        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
            dbo.HAS_VIDEO = AsSbyte(HasVideo);
        }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);

            var agent = session.Agents.FirstOrDefault(x => x.AgentId == AgentId);
            if (agent == null)
                throw new InvalidOperationException(string.Format("Agent {0} is not participating in the session {1}", AgentId, session.Skey));

            var onBehalfOfName = agent.ActsOnBehalfOfAgentId.HasValue
                ? resolver.GetAgentName(session.CustomerId, agent.ActsOnBehalfOfAgentId.Value)
                : agentName;

            var callType = HasVideo ? "Video" : "Audio";
            session.AddSystemMessage(this, false, "{0} call proposal was sent by agent {1}", callType, onBehalfOfName);

            session.MediaCallStatus = MediaCallStatus.ProposedByAgent;
            session.MediaCallAgentHasVideo = HasVideo;
            session.MediaCallVisitorHasVideo = null;
            session.MediaCallAgentId = AgentId;
            session.MediaCallAgentConnectionId = AgentConnectionId;
            session.MediaCallVisitorConnectionId = null;
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var agentInfo = resolver.GetAgentInfo(chatSession.CustomerId, AgentId);
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.MediaCallProposal(chatSessionInfo, agentInfo, messages));
            if (!chatSession.IsOffline && chatSession.VisitorId.HasValue)
            {
                var visitorVisibleMessages = messages.Where(x => !x.IsToAgentsOnly).ToList();
                subscriptionManager.VisitorEventSubscribers.Publish(
                    x => x.MediaCallProposal(chatSessionInfo, agentInfo, visitorVisibleMessages, HasVideo));
            }
        }
    }
}