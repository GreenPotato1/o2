using System;
using System.Collections.Generic;
using Com.O2Bionics.ChatService.Objects;

namespace Com.O2Bionics.ChatService
{
    public interface IAgentSessionStorage : IDbUpdaterStorage<AgentSession>
    {
        AgentSession Get(Guid guid);
        AgentSession GetOrCreate(Guid guid, uint customerId, uint userId);

        HashSet<uint> GetConnectedUsers(uint customerId);

        HashSet<uint> AddConnection(Guid guid, string connectionId);
        HashSet<uint> RemoveConnection(Guid guid, string connectionId);
        void DisconnectAll();
        void AddConnectedSessions(List<KeyValuePair<string, Guid>> list);
    }
}