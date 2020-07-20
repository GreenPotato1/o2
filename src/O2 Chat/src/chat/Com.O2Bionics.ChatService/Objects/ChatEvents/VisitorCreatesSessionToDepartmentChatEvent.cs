using System;
using System.Linq;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;

// ReSharper disable PossibleInvalidOperationException

namespace Com.O2Bionics.ChatService.Objects.ChatEvents
{
    [ChatEventType(ChatEventType.VisitorCreatesSessionToDept)]
    public class VisitorCreatesSessionToDepartmentChatEvent : ChatEventBase
    {
        public VisitorCreatesSessionToDepartmentChatEvent(
            DateTime timestampUtc,
            string text,
            uint targetDepartmentId,
            bool isOfflineSession)
            : base(timestampUtc, text)
        {
            TargetDepartmenID = targetDepartmentId;
            IsOfflineSession = isOfflineSession;
        }

        public VisitorCreatesSessionToDepartmentChatEvent(CHAT_EVENT dbo)
            : base(dbo)
        {
            TargetDepartmenID = dbo.TARGET_DEPARTMENT_ID.Value;
            IsOfflineSession = AsBool(dbo.IS_OFFLINE_SESSION);
        }

        public uint TargetDepartmenID { get; private set; }
        public bool IsOfflineSession { get; private set; }

        public override void Apply(ChatSession session, IObjectResolver resolver)
        {
            var departmentName = resolver.GetDepartmentName(session.CustomerId, TargetDepartmenID);

            session.Status = ChatSessionStatus.Queued;
            session.IsVisitorConnected = true;

            session.Invites.Add(
                new ChatSessionDepartmentInvite(TimestampUtc, null, TargetDepartmenID));

            session.DepartmentsInvolved.Add(TargetDepartmenID);

            session.AddSystemMessage(this, false, "Сессия с департаментом {0} создана", departmentName);
            session.AddVisitorMessage(this);
        }

        protected override void Save(CHAT_EVENT dbo)
        {
            base.Save(dbo);

            dbo.TARGET_DEPARTMENT_ID = TargetDepartmenID;
            dbo.IS_OFFLINE_SESSION = AsSbyte(IsOfflineSession);
        }

        public override void Notify(ChatSession chatSession, IObjectResolver resolver, ISubscriptionManager subscriptionManager)
        {
            var chatSessionInfo = chatSession.AsInfo();
            var messages = chatSession.EventMessagesAsInfo(Id);
            var visitorVisibleMessages = messages.Where(x => !x.IsToAgentsOnly).ToList();
            var visitorInfo = resolver.GetVisitorInfo(chatSession.VisitorId);

            if (!IsOfflineSession && chatSessionInfo.VisitorId.HasValue)
                subscriptionManager.VisitorEventSubscribers.Publish(
                    x => x.VisitorSessionCreated(chatSessionInfo.VisitorId.Value, chatSessionInfo.Skey, visitorVisibleMessages));
            subscriptionManager.AgentEventSubscribers.Publish(
                x => x.VisitorSessionCreated(chatSessionInfo, visitorVisibleMessages, visitorInfo));
        }
    }
}