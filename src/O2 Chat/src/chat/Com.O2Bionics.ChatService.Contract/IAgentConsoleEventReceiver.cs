using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using Com.O2Bionics.Utils.Network;

namespace Com.O2Bionics.ChatService.Contract
{
    [ServiceContract(Namespace = ServiceConstants.Namespace)]
    public interface IAgentConsoleEventReceiver : IPingable
    {
        [OperationContract]
        Dictionary<string, Guid> GetAgentSessionConnections();


        [OperationContract, OneWay]
        void DepartmentStateChanged(uint customerId, List<OnlineStatusInfo> departmentStatus);


        [OperationContract, OneWay]
        void AgentStateChanged(uint customerId, OnlineStatusInfo status);


        [OperationContract, OneWay]
        void UserCreated(uint customerId, UserInfo userInfo);

        [OperationContract, OneWay]
        void UserUpdated(uint customerId, UserInfo userInfo);

        [OperationContract, OneWay]
        void UserRemoved(uint customerId, decimal userId);


        [OperationContract, OneWay]
        void DepartmentCreated(uint customerId, DepartmentInfo deptInfo);

        [OperationContract, OneWay]
        void DepartmentUpdated(uint customerId, DepartmentInfo deptInfo);

        [OperationContract, OneWay]
        void DepartmentRemoved(uint customerId, uint departmentId);


        [OperationContract, OneWay]
        void VisitorSessionCreated(ChatSessionInfo session, List<ChatSessionMessageInfo> messages, VisitorInfo visitor);

        [OperationContract, OneWay]
        void AgentSessionCreated(ChatSessionInfo session, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void AgentSessionAccepted(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void DepartmentSessionAccepted(
            ChatSessionInfo session,
            AgentInfo agent,
            DepartmentInfo targetDepartment,
            List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void AgentSessionRejected(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void VisitorMessage(ChatSessionInfo session, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void AgentMessage(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void AgentInvited(ChatSessionInfo session, AgentInfo agent, AgentInfo invitedAgent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void DepartmentInvited(ChatSessionInfo session, AgentInfo agent, DepartmentInfo invitedDepartment, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void AgentInvitationCanceled(ChatSessionInfo session, AgentInfo agent, AgentInfo invitedAgent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void DepartmentInvitationCanceled(
            ChatSessionInfo session,
            AgentInfo agent,
            DepartmentInfo invitedDept,
            List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void VisitorLeftSession(ChatSessionInfo session, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void AgentLeftSession(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void AgentClosedSession(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void VisitorReconnected(ChatSessionInfo session, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void MediaCallProposal(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void VisitorRejectedMediaCallProposal(ChatSessionInfo session, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void VisitorAcceptedMediaCallProposal(ChatSessionInfo session, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void VisitorStoppedMediaCall(ChatSessionInfo session, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void AgentStoppedMediaCall(ChatSessionInfo session, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void VisitorInfoChanged(
            uint customerId,
            ulong visitorId,
            bool wasRemoved,
            string newName,
            string newEmail,
            string newPhone,
            VisitorSendTranscriptMode? newTranscriptMode,
            long sessionSkey,
            List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void MediaCallVisitorConnectionIdSet(ChatSessionInfo session);

        [OperationContract, OneWay]
        void RtcSendIceCandidate(string agentConnectionId, string candidate);

        [OperationContract, OneWay]
        void RtcSendCallAnswer(string agentConnectionId, string sdp);

        [OperationContract, OneWay]
        void SessionTranscriptSentToVisitor(ChatSessionInfo session, List<ChatSessionMessageInfo> messages);
    }
}