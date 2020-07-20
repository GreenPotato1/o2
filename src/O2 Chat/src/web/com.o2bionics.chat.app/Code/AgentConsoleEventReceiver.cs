using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Web.Console.Hubs;
using Com.O2Bionics.Utils;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Com.O2Bionics.ChatService.Web.Console
{
    [ServiceBehavior(
         ConcurrencyMode = ConcurrencyMode.Multiple,
         InstanceContextMode = InstanceContextMode.Single,
         AddressFilterMode = AddressFilterMode.Any)]
    public class AgentConsoleEventReceiver : IAgentConsoleEventReceiver
    {
        public AgentConsoleEventReceiver()
        {
            Trace();
        }

        public Dictionary<string, Guid> GetAgentSessionConnections()
        {
            return GroupManager.GetAgentSessionConnections();
        }

        public void Ping()
        {
            Trace();
        }

        public void AgentStateChanged(uint customerId, OnlineStatusInfo status)
        {
            Trace(new { customerId, status });

            var connections = GroupManager.GetConnections(
                GroupNames.CustomerGroupName(customerId));
            Clients.Clients(connections)
                .AgentStateChanged(status);
        }

        public void UserCreated(uint customerId, UserInfo userInfo)
        {
            Trace(new { customerId, userInfo });

            var connections = GroupManager.GetConnections(
                GroupNames.CustomerGroupName(customerId));
            Clients.Clients(connections)
                .UserCreated(userInfo);
        }

        public void UserUpdated(uint customerId, UserInfo userInfo)
        {
            Trace(new { customerId, userInfo });

            var connections = GroupManager.GetConnections(
                GroupNames.CustomerGroupName(customerId));
            Clients.Clients(connections)
                .UserUpdated(userInfo);
        }

        public void UserRemoved(uint customerId, decimal skey)
        {
            Trace(new { customerId, skey });

            var connections = GroupManager.GetConnections(
                GroupNames.CustomerGroupName(customerId));
            Clients.Clients(connections)
                .UserRemoved(skey);
        }

        public void DepartmentStateChanged(uint customerId, List<OnlineStatusInfo> departmentStatus)
        {
            Trace(new { customerId, departmentStatus });

            var connections = GroupManager.GetConnections(
                GroupNames.CustomerGroupName(customerId));
            Clients.Clients(connections)
                .DepartmentStateChanged(departmentStatus);
        }

        public void DepartmentCreated(uint customerId, DepartmentInfo deptInfo)
        {
            Trace(new { customerId, deptInfo });

            var connections = GroupManager.GetConnections(
                GroupNames.CustomerGroupName(customerId));
            Clients.Clients(connections)
                .DepartmentCreated(deptInfo);
        }

        public void DepartmentUpdated(uint customerId, DepartmentInfo deptInfo)
        {
            Trace(new { customerId, deptInfo });

            var connections = GroupManager.GetConnections(
                GroupNames.CustomerGroupName(customerId));
            Clients.Clients(connections)
                .DepartmentUpdated(deptInfo);
        }

        public void DepartmentRemoved(uint customerId, uint departmentId)
        {
            Trace(new { customerId, departmentId });

            var connections = GroupManager.GetConnections(
                GroupNames.CustomerGroupName(customerId));
            Clients.Clients(connections)
                .DepartmentRemoved(departmentId);
        }

        public void VisitorSessionCreated(ChatSessionInfo session, List<ChatSessionMessageInfo> messages, VisitorInfo visitor)
        {
            Trace(new { session, messages, visitor });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .VisitorSessionCreated(session, messages, visitor);
        }

        public void AgentSessionCreated(ChatSessionInfo session, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .AgentSessionCreated(session, messages);
        }

        public void AgentSessionAccepted(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .AgentSessionAccepted(session, messages, agent);
        }

        public void DepartmentSessionAccepted(
            ChatSessionInfo session,
            AgentInfo agent,
            DepartmentInfo targetDepartment,
            List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, targetDepartment, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session),
                GroupNames.DepartmentGroupName(targetDepartment.Id));
            Clients.Clients(connections)
                .DepartmentSessionAccepted(session, messages, agent, targetDepartment);
        }

        public void AgentSessionRejected(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session),
                GroupNames.AgentGroupName(agent.Id));
            Clients.Clients(connections)
                .AgentSessionRejected(session, messages, agent);
        }

        public void VisitorMessage(ChatSessionInfo session, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .VisitorMessage(session, messages);
        }

        public void AgentMessage(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .AgentMessage(session, messages, agent);
        }

        public void AgentInvited(ChatSessionInfo session, AgentInfo agent, AgentInfo invitedAgent, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, invitedAgent, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session),
                GroupNames.AgentGroupName(agent.Id));
            Clients.Clients(connections)
                .AgentInvited(session, messages, agent, invitedAgent);
        }

        public void DepartmentInvited(
            ChatSessionInfo session,
            AgentInfo agent,
            DepartmentInfo invitedDepartment,
            List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, invitedDepartment, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session),
                GroupNames.AgentGroupName(agent.Id));
            Clients.Clients(connections)
                .DepartmentInvited(session, messages, agent, invitedDepartment);
        }

        public void AgentInvitationCanceled(ChatSessionInfo session, AgentInfo agent, AgentInfo invitedAgent, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, invitedAgent, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session),
                GroupNames.AgentGroupName(agent.Id));
            Clients.Clients(connections)
                .AgentInvitationCanceled(session, messages, agent, invitedAgent);
        }

        public void DepartmentInvitationCanceled(
            ChatSessionInfo session,
            AgentInfo agent,
            DepartmentInfo invitedDept,
            List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, invitedDept, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session),
                GroupNames.AgentGroupName(agent.Id));
            Clients.Clients(connections)
                .DepartmentInvitationCanceled(session, messages, agent, invitedDept);
        }

        public void VisitorLeftSession(ChatSessionInfo session, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .VisitorLeftSession(session, messages);
        }

        public void AgentLeftSession(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session),
                GroupNames.AgentGroupName(agent.Id));
            Clients.Clients(connections)
                .VisitorLeftSession(session, messages);
        }

        public void AgentClosedSession(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session),
                GroupNames.AgentGroupName(agent.Id));
            Clients.Clients(connections)
                .AgentClosedSession(session, messages);
        }

        // not used now
        public void VisitorReconnected(ChatSessionInfo session, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .VisitorLeftSession(session, messages);
        }

        public void MediaCallProposal(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .MediaCallProposal(session, messages);
        }

        public void VisitorRejectedMediaCallProposal(ChatSessionInfo session, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .VisitorRejectedMediaCallProposal(session, messages);
        }

        public void VisitorAcceptedMediaCallProposal(ChatSessionInfo session, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .VisitorAcceptedMediaCallProposal(session, messages);
        }

        public void VisitorStoppedMediaCall(ChatSessionInfo session, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .VisitorStoppedMediaCall(session, messages);
        }

        public void AgentStoppedMediaCall(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, agent, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .AgentStoppedMediaCall(session, messages);
        }

        public void VisitorInfoChanged(
            uint customerId,
            ulong visitorId,
            bool wasRemoved,
            string newName,
            string newEmail,
            string newPhone,
            VisitorSendTranscriptMode? newTranscriptMode,
            long sessionSkey,
            List<ChatSessionMessageInfo> messages)
        {
            Trace(new { customerId, visitorId, wasRemoved, newName, newEmail, newPhone, newTranscriptMode, sessionSkey, messages });

            var connections = GroupManager.GetConnections(
                GroupNames.CustomerGroupName(customerId));
            Clients.Clients(connections)
                .VisitorInfoChanged(visitorId, wasRemoved, newName, newEmail, newPhone, newTranscriptMode, sessionSkey, messages);
        }

        public void MediaCallVisitorConnectionIdSet(ChatSessionInfo session)
        {
            Trace(new { session });

            if (!string.IsNullOrEmpty(session.MediaCallAgentConnectionId))
                Clients.Client(session.MediaCallAgentConnectionId)
                    .MediaCallVisitorConnectionIdSet(session);
        }

        public void RtcSendIceCandidate(string agentConnectionId, string candidate)
        {
            Trace(new { agentConnectionId });

            Clients.Client(agentConnectionId)
                .RtcSendIceCandidate(candidate);
        }

        public void RtcSendCallAnswer(string agentConnectionId, string sdp)
        {
            Trace(new { agentConnectionId });

            Clients.Client(agentConnectionId)
                .RtcSendCallAnswer(sdp);
        }

        public void SessionTranscriptSentToVisitor(ChatSessionInfo session, List<ChatSessionMessageInfo> messages)
        {
            Trace(new { session, messages });

            var connections = GroupManager.GetConnections(
                SessionGroups(session));
            Clients.Clients(connections)
                .SessionTranscriptSentToVisitor(session, messages);
        }


        private static IEnumerable<string> SessionGroups(ChatSessionInfo session)
        {
            return SessionAgentsIds(session).Select(GroupNames.AgentGroupName)
                .Concat(SessionDepartmentsIDs(session).Select(GroupNames.DepartmentGroupName));
        }

        private static IEnumerable<uint> SessionAgentsIds(ChatSessionInfo session)
        {
            return session.Agents.Select(x => x.AgentId)
                .Union(session.Invites.OfType<ChatSessionAgentInviteInfo>().Where(x => x.IsPending).Select(x => x.AgentId));
        }

        private static IEnumerable<uint> SessionDepartmentsIDs(ChatSessionInfo session)
        {
            return session.Invites.OfType<ChatSessionDepartmentInviteInfo>().Select(x => x.DepartmentId);
        }

        private static IHubConnectionContext<dynamic> Clients
        {
            get
            {
                return GlobalHost.ConnectionManager
                    .GetHubContext<AgentConsoleHub>()
                    .Clients;
            }
        }

        private static readonly ILog m_log = LogManager.GetLogger(typeof(AgentConsoleEventReceiver));

        private static void Trace(object args = null, [CallerMemberName] string methodName = "")
        {
            if (!m_log.IsDebugEnabled) return;

            var argsString = args != null ? args.JsonStringify() : "";
            m_log.DebugFormat(
                "agent event {0}: {1}",
                methodName,
                argsString);
        }
    }
}