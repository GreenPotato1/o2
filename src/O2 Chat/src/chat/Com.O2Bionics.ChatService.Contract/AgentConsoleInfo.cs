using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class AgentConsoleInfo
    {
        [DataMember]
        public uint AgentId { get; set; }

        [DataMember]
        public List<uint> AgentDepartments { get; set; }

        [DataMember]
        public List<DepartmentInfo> Departments { get; set; }

        [DataMember]
        public List<uint> OnlineDepartments { get; set; }

        [DataMember]
        public List<UserInfo> Users { get; set; }

        [DataMember]
        public List<uint> OnlineAgents { get; set; }

        [DataMember]
        public List<ChatSessionInfo> Sessions { get; set; }

        [DataMember]
        public List<VisitorInfo> Visitors { get; set; }

        [DataMember]
        public AgentConsoleSettingsInfo Settings { get; set; }

        [DataMember]
        public uint CustomerId { get; set; }
    }
}