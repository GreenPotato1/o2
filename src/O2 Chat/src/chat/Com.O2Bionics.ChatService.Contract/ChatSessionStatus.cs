using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum ChatSessionStatus
    {
        [EnumMember] Queued = 0,
        [EnumMember] Active = 1,
        [EnumMember] Completed = 2,
    }
}