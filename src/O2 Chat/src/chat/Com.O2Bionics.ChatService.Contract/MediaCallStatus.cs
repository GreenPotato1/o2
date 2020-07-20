using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum MediaCallStatus
    {
        [EnumMember] None = 0,
        [EnumMember] ProposedByAgent = 1,
        [EnumMember] AcceptedByVisitor = 2,
        [EnumMember] Established = 3,
    }
}