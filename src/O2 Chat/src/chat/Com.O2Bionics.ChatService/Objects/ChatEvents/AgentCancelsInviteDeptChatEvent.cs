using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentCancelsInviteDept)]
    public class AgentCancelsInviteDeptChatEvent : ChatEventBase
    {
        public AgentCancelsInviteDeptChatEvent(
            DateTime timestampUtc,
            string text,
            uint agentId,
            uint invitedDepartmentId)
            : base(timestampUtc, text)
        {
            AgentId = agentId;
            InvitedDepartmentId = invitedDepartmentId;
        }

        public AgentCancelsInviteDeptChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
            InvitedDepartmentId = dbo.TARGET_DEPARTMENT_ID.Value;
        }

        public uint AgentId { get; private set; }
        public uint InvitedDepartmentId { get; private set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            var invite = session.Invites
                .OfType<ChatSessionDepartmentInvite>()
                .FirstOrDefault(x => x.IsPending && x.DepartmentId == InvitedDepartmentId);
            if (invite == null)
                return;

            invite.Cancel(TimestampUtc, AgentId);

            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);
            var invitedDepartmentName = resolver.GetDepartmentName(session.CustomerId, InvitedDepartmentId);
            session.AddSystemMessage(
                this,
                invite.ActOnBehalfOfAgentId.HasValue,
                "Agent {0} has been canceled the invitation for the department {1}",
                agentName,
                invitedDepartmentName);
        }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
            dbo.TARGET_DEPARTMENT_ID = InvitedDepartmentId;
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var agentInfo = resolver.GetAgentInfo(chatSession.CustomerId, AgentId);
            var invitedDepartmentInfo = resolver.GetDepartmentInfo(chatSession.CustomerId, InvitedDepartmentId);
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.DepartmentInvitationCanceled(chatSessionInfo, agentInfo, invitedDepartmentInfo, messages));
        }
    }
}