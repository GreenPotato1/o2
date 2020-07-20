using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum ChatEventType
    {
        [EnumMember] VisitorCreatesSessionToDept = 1,
        [EnumMember] AgentCreatesSessionToDept = 2,
        [EnumMember] AgentCreatesSessionToAgent = 3,

        [EnumMember] AgentInvitesAgent = 4,
        [EnumMember] AgentInvitesDept = 5,

        [EnumMember] AgentCancelsInviteAgent = 6,
        [EnumMember] AgentCancelsInviteDept = 7,

        [EnumMember] AgentAcceptsAgentSession = 8,
        [EnumMember] AgentAcceptsDeptSession = 9,

        [EnumMember] AgentRejectsAgentSession = 10,

        [EnumMember] AgentLeavesSession = 12,
        [EnumMember] VisitorLeavesSession = 13,

        [EnumMember] AgentSendsMessage = 14,
        [EnumMember] VisitorSendsMessage = 15,

        [EnumMember] VisitorReconnect = 16,

        [EnumMember] VisitorUpdatesInfo = 17,

        [EnumMember] AgentClosesSession = 18,

        [EnumMember] VisitorAcceptedMediaCallProposal = 19,
        [EnumMember] VisitorRejectedMediaCallProposal = 20,
        [EnumMember] VisitorStoppedMediaCall = 21,
        [EnumMember] VisitorSetMediaCallConnectionId = 22,

        [EnumMember] AgentMediaCallProposal = 23,
        [EnumMember] AgentStoppedMediaCall = 24,

        [EnumMember] SessionTranscriptSentToVisitor = 25,
        [EnumMember] VisitorRequestedTranscriptSent = 26,
    }
}