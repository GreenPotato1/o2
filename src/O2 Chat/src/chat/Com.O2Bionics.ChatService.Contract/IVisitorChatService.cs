using System.Collections.Generic;
using System.ServiceModel;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Contract
{
    /// <summary>
    /// It is called from the widget "chatframe.cshtml".
    /// </summary>
    [ServiceContract(Namespace = ServiceConstants.Namespace)]
    public interface IVisitorChatService
#if ERRORTRACKERTEST
        : IErrorTrackerTest
#endif
    {
        [OperationContract]
        void Subscribe(string endpoint);

        [OperationContract]
        void Unsubscribe(string endpoint);

        [OperationContract]
        [CanBeNull]
        List<uint> GetActiveCustomerIds();

        [OperationContract]
        [NotNull]
        ChatFrameLoadResult ChatWindowLoad(uint customerId, ulong visitorId, [NotNull] string domain, bool isDemoMode);

        [OperationContract]
        ChatWindowOpenResult ChatWindowOpen(uint customerId, ulong visitorId, string historyId, MediaSupport mediaSupport);

        [OperationContract]
        long ResetWidgetLoads([NotNull] uint[] customerIds);


        [OperationContract]
        void UpdateVisitorInfo(uint customerId, ulong visitorId, VisitorInfo visitorInfo);

        [OperationContract]
        void ClearVisitorInfo(uint customerId, ulong visitorId);


        [OperationContract]
        CustomerSettingsInfo GetCustomerSettings(uint customerId);

        [OperationContract]
        bool IsCaptchaRequired(uint customerId);

        [OperationContract]
        void StartChatSession(uint customerId, ulong visitorId, uint departmentId, bool isOfflineSession, string messageText);

        [OperationContract]
        void SendMessage(ulong visitorId, string messageText);

        [OperationContract]
        void LeaveChatSession(ulong visitorId);


        [OperationContract]
        void OnReconnected(ulong visitorId);

        [OperationContract]
        void OnDisconnected(uint customerId, ulong visitorId, string connectionId, bool stopCalled);


        [OperationContract]
        void MediaCallProposalRejected(ulong visitorId);

        [OperationContract]
        void MediaCallProposalAccepted(ulong visitorId, bool hasVideo);

        [OperationContract]
        void MediaCallSetConnectionId(ulong visitorId, uint customerId, string connectionId);

        [OperationContract]
        void MediaCallStop(ulong visitorId, string reason);

        [OperationContract]
        void RtcSendIceCandidate(string agentConnectionId, string candidate);

        [OperationContract]
        void RtcSendCallAnswer(string agentConnectionId, string sdp);

        [OperationContract]
        CallResultStatus SendTranscript(ulong visitorId, uint customerId, long sessionId, int visitorTimezoneOffsetMinutes);
    }
}