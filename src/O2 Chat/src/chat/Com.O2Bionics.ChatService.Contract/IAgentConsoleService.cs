using System;
using System.ServiceModel;
using Com.O2Bionics.Utils.Network;

namespace Com.O2Bionics.ChatService.Contract
{
    [ServiceContract(Namespace = ServiceConstants.Namespace)]
    public interface IAgentConsoleService
    {
        [OperationContract]
        void Subscribe(string endpoint);

        [OperationContract]
        void Unsubscribe(string endpoint);


        [OperationContract]
        AgentSessionConnectResult Connected(
            uint customerId,
            Guid agentSessionGuid,
            uint agentId,
            string connectionId);

        [OperationContract]
        void Disconnected(
            uint customerId,
            Guid agentSessionGuid,
            uint agentId,
            string connectionId);

        [OperationContract]
        void SessionSetStatus(
            uint customerId,
            Guid agentSessionGuid,
            bool isOnline);


        [OperationContract]
        AgentConsoleInfo GetConsoleInfo(
            uint customerId,
            Guid agentSessionGuid);

        [OperationContract]
        FullChatSessionInfo GetFullChatSessionInfo(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey);

        [OperationContract]
        VisitorInfo GetVisitorInfo(
            uint customerId,
            Guid agentSessionGuid,
            ulong visitorId);


        [OperationContract]
        void StartChatSessionToAgent(
            uint customerId,
            Guid agentSessionGuid,
            uint targetAgentId,
            string messageText);

        [OperationContract]
        void StartChatSessionToDepartment(
            uint customerId,
            Guid agentSessionGuid,
            uint targetDepartmentId,
            string messageText);

        [OperationContract]
        void AcceptAgentChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey);

        [OperationContract]
        void AcceptDepartmentChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            uint departmentId);

        [OperationContract]
        void RejectAgentChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey);

        [OperationContract]
        void InviteAgentToChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            uint invitedAgentId,
            bool actOnBehalfOfInvitor,
            string messageText);

        [OperationContract]
        void CancelAgentInvitationToChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            uint invitedAgentId,
            string messageText);

        [OperationContract]
        void InviteDepartmentToChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            uint invitedDepartmentId,
            bool actOnBehalfOfInvitor,
            string messageText);

        [OperationContract]
        void CancelDepartmentInvitationToChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            uint invitedDepartmentId,
            string messageText);

        [OperationContract]
        void SendMessageToChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            bool isToAgentsOnly,
            string messageText);

        [OperationContract]
        void LeaveChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            string messageText,
            bool isDisconnected,
            bool isBecameOffline);

        [OperationContract]
        void CloseChatSession(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            string text);

        [OperationContract]
        void SendTranscriptToVisitor(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            int visitorTimezoneOffsetMinutes);


        [OperationContract]
        void MediaCallProposal(
            uint customerId,
            Guid agentSessionGuid,
            string agentConnectionId,
            long chatSessionSkey,
            bool hasVideo);

        [OperationContract]
        void MediaCallStop(
            uint customerId,
            Guid agentSessionGuid,
            long chatSessionSkey,
            string reason);


        [OperationContract]
        void RtcSendIceCandidate(
            uint customerId,
            Guid agentSessionGuid,
            string visitorConnectionId,
            string candidate);

        [OperationContract]
        void RtcSendCallOffer(
            uint customerId,
            Guid agentSessionGuid,
            string visitorConnectionId,
            string sdp);
    }
}