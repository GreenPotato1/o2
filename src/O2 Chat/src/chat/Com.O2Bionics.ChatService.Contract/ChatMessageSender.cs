using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum ChatMessageSender
    {
        [EnumMember] System = 1,
        [EnumMember] Visitor = 2,
        [EnumMember] Agent = 3,
    }
}