using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class OnlineStatusInfo
    {
        [DataMember]
        public uint Id { get; set; }

        [DataMember]
        public bool IsOnline { get; set; }
    }
}