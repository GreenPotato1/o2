using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum AccountLookupStatus
    {
        //If you change this enumeration, make changes to the "OperationStatus.cs".

        [EnumMember] Success,
        [EnumMember] NotFound,
        [EnumMember] NotActive,
        [EnumMember] CustomerNotActive
    }
}