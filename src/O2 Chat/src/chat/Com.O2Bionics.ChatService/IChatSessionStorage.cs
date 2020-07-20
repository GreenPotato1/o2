using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.ChatService.Objects.ChatEvents;

namespace Com.O2Bionics.ChatService
{
    public interface IChatSessionStorage : IDbUpdaterStorage<ChatSession>
    {
        int Count();

        ChatSession Get(uint customerId, long skey);
        ChatSession GetVisitorOnlineSession(uint customerId, ulong visitorId);

        ChatSession CreateNew(uint customerId, ChatEventBase chatEvent, bool isOffline, ulong? visitorId);
        ChatSession AddEvent(uint customerId, long skey, ChatEventBase chatEvent);
        ChatSession AddVisitorOnlineEvent(uint customerId, ulong visitorId, ChatEventBase chatEvent);

        List<ChatSession> GetAgentVisibleSessions(uint customerId, uint agentId, HashSet<uint> departmentIDs);
        List<ChatSession> GetAgentSessions(uint customerId, uint agentId);

        List<ChatSession> Search(
            uint customerId,
            decimal userId,
            HashSet<uint> visibleDepartments,
            SessionSearchFilter filter,
            int pageSize,
            int pageNumber);

        List<ChatSessionMessage> GetMessages(
            uint customerId,
            long sessionSkey,
            int pageSize,
            int pageNumber);
    }
}