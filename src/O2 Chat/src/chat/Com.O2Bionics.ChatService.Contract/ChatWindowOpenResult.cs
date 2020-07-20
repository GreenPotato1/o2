using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class ChatWindowOpenResult
    {
        [DataMember]
        public VisitorInfo Visitor { get; set; }

        [DataMember]
        public List<DepartmentInfo> Departments { get; set; }

        [DataMember]
        public List<uint> OnlineDepartments { get; set; }

        [DataMember]
        public bool HasActiveSession { get; set; }

        [DataMember]
        public ChatSessionInfo Session { get; set; }

        [DataMember]
        public List<ChatSessionMessageInfo> SessionMessages { get; set; }

        [DataMember]
        public List<AgentInfo> Agents { get; set; }
    }
}