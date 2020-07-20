using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum VisitorSendTranscriptMode
    {
        [EnumMember] Ask = 0,
        [EnumMember] Always = 1,
        [EnumMember] Never = 2,
    }
}