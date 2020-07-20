using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.AgentInvitesDept)]
    public class AgentInvitesDepartmentChatEvent : ChatEventBase
    {
        public AgentInvitesDepartmentChatEvent(
            DateTime timestampUtc,
            string text,
            uint agentId,
            uint invitedDepartmentId,
            bool actOnBehalfOfInvitor)
            : base(timestampUtc, text)
        {
            AgentId = agentId;
            InvitedDepartmentId = invitedDepartmentId;
            ActOnBehalfOfInvitor = actOnBehalfOfInvitor;
        }

        public AgentInvitesDepartmentChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            AgentId = dbo.AGENT_ID.Value;
            InvitedDepartmentId = dbo.TARGET_DEPARTMENT_ID.Value;
            ActOnBehalfOfInvitor = AsBool(dbo.ACT_ON_BEHALF_OF_INVITOR);
        }

        public uint AgentId { get; private set; }
        public uint InvitedDepartmentId { get; private set; }
        public bool ActOnBehalfOfInvitor { get; private set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            session.Status = ChatSessionStatus.Active;

            if (session.Invites.OfType<ChatSessionDepartmentInvite>().Any(x => x.IsPending && x.DepartmentId == InvitedDepartmentId))
                return;
            session.Invites.Add(
                new ChatSessionDepartmentInvite(
                    TimestampUtc,
                    AgentId,
                    InvitedDepartmentId,
                    ActOnBehalfOfInvitor ? AgentId : (uint?)null));

            session.DepartmentsInvolved.Add(InvitedDepartmentId);

            var agentName = resolver.GetAgentName(session.CustomerId, AgentId);
            var invitedDepartmentName = resolver.GetDepartmentName(session.CustomerId, InvitedDepartmentId);
            session.AddSystemMessage(this, ActOnBehalfOfInvitor, "Агент {0} пригласил {1} к сессии", agentName, invitedDepartmentName);
        }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.AGENT_ID = AgentId;
            dbo.TARGET_DEPARTMENT_ID = InvitedDepartmentId;
            dbo.ACT_ON_BEHALF_OF_INVITOR = AsSbyte(ActOnBehalfOfInvitor);
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var agentInfo = resolver.GetAgentInfo(chatSession.CustomerId, AgentId);
            var invitedDepartmentInfo = resolver.GetDepartmentInfo(chatSession.CustomerId, InvitedDepartmentId);
            var messages = chatSession.EventMessagesAsInfo(Id);

            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.DepartmentInvited(chatSessionInfo, agentInfo, invitedDepartmentInfo, messages));
        }
    }
}