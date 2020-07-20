using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Contract
{
    [ServiceContract(Namespace = ServiceConstants.Namespace)]
    public interface IVisitorChatEventReceiver : IPingable
    {
        [OperationContract, OneWay]
        void DepartmentStateChanged(uint customerId, List<OnlineStatusInfo> departmentStatus);

        [OperationContract, OneWay]
        void CustomersChanged(DateTime date, [NotNull] IList<KeyValuePair<uint, CustomerEntry>> customerIdEntries);

        [OperationContract, OneWay]
        void AgentSessionAccepted(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void DepartmentSessionAccepted(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void AgentMessage(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void AgentLeftSession(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void AgentClosedSession(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void MediaCallProposal(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages, bool hasVideo);

        [OperationContract, OneWay]
        void VisitorAcceptedMediaCallProposal(ulong visitorId);

        [OperationContract, OneWay]
        void VisitorRejectedMediaCallProposal(ulong visitorId);

        [OperationContract, OneWay]
        void VisitorStoppedMediaCall(ChatSessionInfo sessionInfo, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void RtcSendIceCandidate(string visitorConnectionId, string candidate);

        [OperationContract, OneWay]
        void RtcSendCallOffer(string visitorConnectionId, string sdp);

        [OperationContract, OneWay]
        void AgentStoppedMediaCall(ChatSessionInfo sessionInfo, AgentInfo agent, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void VisitorInfoChanged(
            ulong visitorId,
            bool wasRemoved,
            string newName,
            string newEmail,
            string newPhone,
            VisitorSendTranscriptMode? newTranscriptMode);

        [OperationContract, OneWay]
        void VisitorSessionCreated(ulong visitorId, long sessionId, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void VisitorMessage(ulong visitorId, List<ChatSessionMessageInfo> messages);

        [OperationContract, OneWay]
        void VisitorRequestedTranscriptSent(ChatSessionInfo chatSessionInfo, List<ChatSessionMessageInfo> messages);
    }
}