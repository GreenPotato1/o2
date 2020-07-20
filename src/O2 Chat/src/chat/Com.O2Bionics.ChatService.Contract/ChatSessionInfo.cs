using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Jil;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class ChatSessionInfo
    {
        [DataMember]
        public long Skey { get; set; }

        [DataMember]
        [JilDirective(TreatEnumerationAs = typeof(int))]
        public ChatSessionStatus Status { get; set; }

        [DataMember]
        public bool IsOffline { get; set; }

        [DataMember]
        public bool IsVisitorConnected { get; set; }

        [DataMember]
        public long? VisitorTranscriptLastEvent { get; set; }

        [DataMember]
        public DateTime? VisitorTranscriptTimestampUtc { get; set; }

        [DataMember]
        public DateTime AddTimestampUtc { get; set; }

        [DataMember]
        public DateTime? AnswerTimestampUtc { get; set; }

        [DataMember]
        public DateTime? EndTimestampUtc { get; set; }

        [DataMember]
        public DateTime LastEventTimestampUtc { get; set; }

        [DataMember]
        [JilDirective(TreatEnumerationAs = typeof(int))]
        public MediaCallStatus MediaCallStatus { get; set; }

        [DataMember]
        public uint MediaCallAgentId { get; set; }

        [DataMember]
        public bool? MediaCallAgentHasVideo { get; set; }

        [DataMember]
        public bool? MediaCallVisitorHasVideo { get; set; }

        [DataMember]
        public string MediaCallAgentConnectionId { get; set; }

        [DataMember]
        public string MediaCallVisitorConnectionId { get; set; }

        [DataMember]
        public ulong? VisitorId { get; set; }

        [DataMember]
        public List<ChatSessionInviteInfo> Invites { get; set; }

        [DataMember]
        public List<ChatSessionAgentInfo> Agents { get; set; }

        [DataMember]
        public HashSet<decimal> AgentsInvolved { get; set; }

        [DataMember]
        public HashSet<uint> DepartmentsInvolved { get; set; }

        [DataMember]
        public int VisitorMessageCount { get; set; }

        [DataMember]
        public int AgentMessageCount { get; set; }
    }
}