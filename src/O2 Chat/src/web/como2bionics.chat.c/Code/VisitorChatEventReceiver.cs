using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Web.Chat.Hubs;
using Com.O2Bionics.ChatService.Widget;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Com.O2Bionics.ChatService.Web.Chat
{
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single,
        AddressFilterMode = AddressFilterMode.Any)]
    public class VisitorChatEventReceiver : IVisitorChatEventReceiver
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(VisitorChatEventReceiver));
        private readonly ICustomerCache m_customerCache;

        public VisitorChatEventReceiver([NotNull] ICustomerCache customerCache)
        {
            m_customerCache = customerCache ?? throw new ArgumentNullException(nameof(customerCache));

            Debug.WriteLine("visitor: created");
        }

        public void Ping()
        {
            Debug.WriteLine("visitor: ping from service");
        }

        public void CustomersChanged(DateTime date, IList<KeyValuePair<uint, CustomerEntry>> customerIdEntries)
        {
            if (null == customerIdEntries || 0 == customerIdEntries.Count)
                throw new ArgumentNullException(nameof(customerIdEntries));

            if (m_log.IsDebugEnabled)
                m_log.Debug($"{customerIdEntries.Count} customers changed for {date}");

            Debug.Assert(date == date.RemoveTime());
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < customerIdEntries.Count; i++)
            {
                var p = customerIdEntries[i];
                m_customerCache.Set(date, p.Key, p.Value);
            }
        }

        public void DepartmentStateChanged(uint customerId, List<OnlineStatusInfo> departmentStatus)
        {
            Debug.WriteLine("visitor: DepartmentStateChanged");

            Clients.Group(GroupNames.CustomerGroupName(customerId))
                .DepartmentStateChanged(departmentStatus);
        }

        public void AgentSessionAccepted(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Debug.WriteLine("visitor: AgentSessionAccepted");

            if (sessionInfo.VisitorId != null)
                Clients.Group(GroupNames.VisitorGroupName(sessionInfo.VisitorId.Value))
                    .AgentSessionAccepted(sessionInfo, agent, messages);
        }

        public void DepartmentSessionAccepted(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Debug.WriteLine("visitor: DepartmentSessionAccepted");

            if (sessionInfo.VisitorId != null)
                Clients.Group(GroupNames.VisitorGroupName(sessionInfo.VisitorId.Value))
                    .DepartmentSessionAccepted(sessionInfo, agent, messages);
        }

        public void AgentMessage(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Debug.WriteLine("visitor: AgentMessage");

            if (sessionInfo.VisitorId != null)
                Clients.Group(GroupNames.VisitorGroupName(sessionInfo.VisitorId.Value))
                    .AgentMessage(sessionInfo, agent, messages);
        }

        public void AgentLeftSession(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Debug.WriteLine("visitor: AgentLeftSession");

            if (sessionInfo.VisitorId != null)
                Clients.Group(GroupNames.VisitorGroupName(sessionInfo.VisitorId.Value))
                    .AgentLeftSession(sessionInfo, agent, messages);
        }

        public void AgentClosedSession(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Debug.WriteLine("visitor: AgentClosedSession");

            if (sessionInfo.VisitorId != null)
                Clients.Group(GroupNames.VisitorGroupName(sessionInfo.VisitorId.Value))
                    .AgentClosedSession(sessionInfo, agent, messages);
        }

        public void MediaCallProposal(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages, bool hasVideo)
        {
            Debug.WriteLine("visitor: MediaCallProposal");

            if (sessionInfo.VisitorId != null)
                Clients.Group(GroupNames.VisitorGroupName(sessionInfo.VisitorId.Value))
                    .MediaCallProposal(sessionInfo, agent, messages, hasVideo);
        }

        public void VisitorAcceptedMediaCallProposal(ulong visitorId)
        {
            Debug.WriteLine("visitor: VisitorAcceptedMediaCallProposal");

            Clients.Group(GroupNames.VisitorGroupName(visitorId))
                .VisitorAcceptedMediaCallProposal();
        }

        public void VisitorRejectedMediaCallProposal(ulong visitorId)
        {
            Debug.WriteLine("visitor: VisitorRejectedMediaCallProposal");

            Clients.Group(GroupNames.VisitorGroupName(visitorId))
                .VisitorRejectedMediaCallProposal();
        }

        public void VisitorStoppedMediaCall(ChatSessionInfo sessionInfo, List<ChatSessionMessageInfo> messages)
        {
            Debug.WriteLine("visitor: VisitorStoppedMediaCall");

            if (sessionInfo.VisitorId != null)
                Clients.Group(GroupNames.VisitorGroupName(sessionInfo.VisitorId.Value))
                    .VisitorStoppedMediaCall(sessionInfo, messages);
        }

        public void AgentStoppedMediaCall(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Debug.WriteLine("visitor: AgentStoppedMediaCall");

            if (sessionInfo.VisitorId != null)
                Clients.Group(GroupNames.VisitorGroupName(sessionInfo.VisitorId.Value))
                    .AgentStoppedMediaCall(sessionInfo, agent, messages);
        }

        public void VisitorInfoChanged(
            ulong visitorId,
            bool wasRemoved,
            string newName,
            string newEmail,
            string newPhone,
            VisitorSendTranscriptMode? newTranscriptMode)
        {
            Debug.WriteLine("visitor: VisitorInfoChanged");

            Clients.Group(GroupNames.VisitorGroupName(visitorId))
                .VisitorInfoChanged(wasRemoved, newName, newEmail, newPhone, newTranscriptMode);
        }

        public void VisitorSessionCreated(ulong visitorId, long sessionId, List<ChatSessionMessageInfo> messages)
        {
            Debug.WriteLine("visitor: VisitorSessionCreated");

            Clients.Group(GroupNames.VisitorGroupName(visitorId))
                .VisitorSessionCreated(sessionId, messages);
        }

        public void VisitorMessage(ulong visitorId, List<ChatSessionMessageInfo> messages)
        {
            Debug.WriteLine("visitor: VisitorMessage");

            Clients.Group(GroupNames.VisitorGroupName(visitorId))
                .VisitorMessage(messages);
        }

        public void RtcSendIceCandidate(string visitorConnectionId, string candidate)
        {
            Debug.WriteLine("visitor: RtcSendIceCandidate");

            Clients.Client(visitorConnectionId)
                .RtcSendIceCandidate(candidate);
        }

        public void RtcSendCallOffer(string visitorConnectionId, string sdp)
        {
            Debug.WriteLine("visitor: RtcSendCallOffer");

            Clients.Client(visitorConnectionId)
                .RtcSendCallOffer(sdp);
        }

        public void VisitorRequestedTranscriptSent(ChatSessionInfo sessionInfo, List<ChatSessionMessageInfo> messages)
        {
            Debug.WriteLine("visitor: VisitorRequestedTranscriptSent");
            if (sessionInfo.VisitorId != null)
                Clients.Group(GroupNames.VisitorGroupName(sessionInfo.VisitorId.Value))
                    .VisitorRequestedTranscriptSent(sessionInfo, messages);
        }

        private static IHubConnectionContext<dynamic> Clients =>
            GlobalHost.ConnectionManager
                .GetHubContext<VisitorChatHub>()
                .Clients;
    }
}