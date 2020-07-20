using System.Collections.Generic;
using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService
{
    public interface IObjectResolver
    {
        string GetDepartmentName(uint customerId, uint id);
        string GetAgentName(uint customerId, uint id);

        AgentInfo GetAgentInfo(uint customerId, uint? id);
        HashSet<uint> GetAgentDepartments(uint customerId, uint? id);
        DepartmentInfo GetDepartmentInfo(uint customerId, uint? id);
        VisitorInfo GetVisitorInfo(ulong? id);
    }
}