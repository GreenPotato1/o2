using System;
using System.Runtime.Serialization;
using Jil;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    [KnownType(typeof(ChatSessionAgentInviteInfo))]
    [KnownType(typeof(ChatSessionDepartmentInviteInfo))]
    public class ChatSessionInviteInfo
    {
        [DataMember]
        public DateTime CreatedTimestampUtc { get; set; }

        [DataMember]
        public uint? CreatorAgentId { get; set; }

        [DataMember]
        [JilDirective(TreatEnumerationAs = typeof(int))]
        public ChatSessionInviteType InviteType { get; set; }

        [DataMember]
        public uint? ActOnBehalfOfAgentId { get; set; }

        [DataMember]
        public DateTime? AcceptedTimestampUtc { get; set; }

        [DataMember]
        public uint? AcceptedByAgentId { get; set; }

        [DataMember]
        public DateTime? CanceledTimestampUtc { get; set; }

        [DataMember]
        public uint? CanceledByAgentId { get; set; }

        public bool IsAccepted
        {
            get { return AcceptedByAgentId.HasValue; }
        }

        public bool IsCanceled
        {
            get { return CanceledByAgentId.HasValue; }
        }

        public bool IsPending
        {
            get { return !IsAccepted && !IsCanceled; }
        }
    }

    [DataContract]
    public class ChatSessionAgentInviteInfo : ChatSessionInviteInfo
    {
        [DataMember]
        public uint AgentId { get; set; }
    }

    [DataContract]
    public class ChatSessionDepartmentInviteInfo : ChatSessionInviteInfo
    {
        [DataMember]
        public uint DepartmentId { get; set; }
    }
}