using System;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentCreatesSessionToDept)]
    public class AgentCreatesSessionToDepartmentChatEvent : AgentCreatesSessionChatEventBase
    {
        public AgentCreatesSessionToDepartmentChatEvent(
            DateTime timestampUtc,
            string text,
            uint agentId,
            uint targetDepartmentId)
            : base(timestampUtc, text)
        {
            AgentId = agentId;
            TargetDepartmentId = targetDepartmentId;
        }

        public AgentCreatesSessionToDepartmentChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
            TargetDepartmentId = dbo.TARGET_DEPARTMENT_ID.Value;
        }

        public uint AgentId { get; private set; }
        public uint TargetDepartmentId { get; private set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            if (session.IsOffline)
                throw new InvalidOperationException("Agent can't start offline session to Department");

            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);
            var departmentName = resolver.GetDepartmentName(session.CustomerId, TargetDepartmentId);

            session.Status = ChatSessionStatus.Queued;

            session.Invites.Add(
                new ChatSessionDepartmentInvite(TimestampUtc, AgentId, TargetDepartmentId));
            session.Agents.Add(new ChatSessionAgent(AgentId));

            session.DepartmentsInvolved.Add(TargetDepartmentId);

            session.AddSystemMessage(this, false, "Session to department {0} has been created", departmentName);
            session.AddAgentMessage(this, AgentId, agentName);
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
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.AgentSessionCreated(chatSessionInfo, messages));
        }
    }
}