using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public enum WidgetLoadSatusCode
    {
        [EnumMember] Allowed,
        [EnumMember] DisabledCustomer,
        [EnumMember] ViewCounterExceeded,
        [EnumMember] UnknownDomain,
        [EnumMember] UnknownDomainNumberExceeded
    }
}