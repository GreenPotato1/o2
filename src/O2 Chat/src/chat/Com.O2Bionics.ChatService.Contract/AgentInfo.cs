using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class AgentInfo : IHasCompositeFullName
    {
        [DataMember]
        public uint Id { get; set; }

        [DataMember]
        public uint CustomerId { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Avatar { get; set; }
    }
}