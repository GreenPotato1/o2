using System;
using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Objects;

namespace Com.O2Bionics.ChatService
{
    public interface IAgentManager
    {
        void Stop();
        void Start();

        AgentSession GetAgentSession(Guid agentSessionGuid);

        List<DepartmentInfo> GetCustomerDepartmentsInfo(ChatDatabase db, uint customerId, bool skipPrivate);
        HashSet<uint> GetCustomerOnlineDepartments(uint customerId);
        HashSet<uint> GetCustomerOnlineAgents(uint customerId);
        HashSet<uint> GetAgentDepartmentIds(ChatDatabase db, uint customerId, uint agentId);

        AgentSessionConnectResult Connect(ChatDatabase db, Guid agentSessionGuid, uint customerId, uint agentId, string connectionId);
        void Disconnect(uint customerId, Guid agentSessionGuid, uint agentId, string connectionId);
        void SetUserOnlineStatus(ChatDatabase db, uint customerId, Guid agentSessionGuid, bool isOnline);
        void DisconnectAll();
    }
}