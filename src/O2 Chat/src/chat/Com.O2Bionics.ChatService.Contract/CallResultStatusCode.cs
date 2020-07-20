using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum CallResultStatusCode
    {
        [EnumMember] Success = 0,
        [EnumMember] AccessDenied = 1,
        [EnumMember] Warning = 2,
        [EnumMember] Failure = 3,
        [EnumMember] NotFound = 4,
        [EnumMember] ValidationFailed = 5,
    }
}