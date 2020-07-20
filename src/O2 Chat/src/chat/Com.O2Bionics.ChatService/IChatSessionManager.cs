using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.ChatService.Objects.ChatEvents;

namespace Com.O2Bionics.ChatService
{
    public interface IChatSessionManager
    {
        ChatSession GetVisitorOnlineSession(uint customerId, ulong visitorId);
        List<ChatSessionMessageInfo> GetVisitorOnlineSessionMessages(uint customerId, ulong visitorId);
        bool HasActiveOnlineSession(List<ChatSessionMessageInfo> sessionMessages);

        ChatSession GetSession(uint customerId, long sessionSkey);
        FullChatSessionInfo GetFullSessionInfo(uint customerId, long sessionSkey);
        List<ChatSessionInfo> GetAgentVisibleSessions(uint customerId, uint agentId, HashSet<uint> departmentIDs);
        List<ChatSessionInfo> GetAgentSessions(uint customerId, uint agentId);

        void CreateNewSession(uint customerId, ChatEventBase chatEvent, bool isOffline = false, ulong? visitorId = null);
        void AddEvent(uint customerId, long chatSessionSkey, ChatEventBase chatEvent);
        void AddVisitorOnlineEvent(ulong visitorId, ChatEventBase chatEvent);
    }
}