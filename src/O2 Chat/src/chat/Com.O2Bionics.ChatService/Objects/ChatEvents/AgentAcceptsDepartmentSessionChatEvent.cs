using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentAcceptsDeptSession)]
    public class AgentAcceptsDepartmentSessionChatEvent : ChatEventBase
    {
        public AgentAcceptsDepartmentSessionChatEvent(
            DateTime timestampUtc,
            uint agentId,
            uint targetDepartmentId)
            : base(timestampUtc, null)
        {
            AgentId = agentId;
            TargetDepartmentId = targetDepartmentId;
        }

        public AgentAcceptsDepartmentSessionChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
            TargetDepartmentId = dbo.TARGET_DEPARTMENT_ID.Value;
        }

        public uint AgentId { get; private set; }
        public uint TargetDepartmentId { get; private set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);

            var invite = session.Invites
                .OfType<ChatSessionDepartmentInvite>()
                .FirstOrDefault(x => x.IsPending && x.DepartmentId == TargetDepartmentId);

            if (invite == null)
                throw new InvalidOperationException(
                    string.Format(
                        "Session state: no pending invites for event {0}, department {1}, in session {2}",
                        Id,
                        TargetDepartmentId,
                        session.Skey));

            session.Status = ChatSessionStatus.Active;

            invite.Accept(TimestampUtc, AgentId);
            session.Agents.Add(new ChatSessionAgent(AgentId, invite.ActOnBehalfOfAgentId));

            session.AgentsInvolved.Add(AgentId);
            if (invite.ActOnBehalfOfAgentId.HasValue)
                session.AgentsInvolved.Add(invite.ActOnBehalfOfAgentId.Value);

            session.AddSystemMessage(this, invite.ActOnBehalfOfAgentId.HasValue, "Агент {0} принял сессию", agentName);
        }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
            dbo.TARGET_DEPARTMENT_ID = TargetDepartmentId;
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var agentInfo = resolver.GetAgentInfo(chatSession.CustomerId, AgentId);
            var targetDepartmentInfo = resolver.GetDepartmentInfo(chatSession.CustomerId, TargetDepartmentId);
            var messages = chatSession.EventMessagesAsInfo(Id);
            var visitorVisibleMessages = messages.Where(x => !x.IsToAgentsOnly).ToList();

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.DepartmentSessionAccepted(chatSessionInfo, agentInfo, targetDepartmentInfo, messages));
            if (!chatSession.IsOffline && chatSession.VisitorId.HasValue)
                subscriptionManager.VisitorEventSubscribers.Publish(
                    x => x.DepartmentSessionAccepted(chatSessionInfo, agentInfo, visitorVisibleMessages));
        }
    }
}