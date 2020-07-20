using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum ChatSessionInviteType
    {
        [EnumMember] Department = 1,
        [EnumMember] Agent = 2,
    }
}