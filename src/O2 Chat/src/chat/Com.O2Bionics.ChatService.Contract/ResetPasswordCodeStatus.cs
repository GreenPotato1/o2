using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum ResetPasswordCodeStatus
    {
        [EnumMember] Success,
        [EnumMember] CodeNotFoundOrExpired,
        [EnumMember] AccountRemovedOrLocked,
    }
}