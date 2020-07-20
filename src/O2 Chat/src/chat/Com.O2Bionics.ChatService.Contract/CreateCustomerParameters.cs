using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class CreateCustomerParameters
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string CustomerName { get; set; }

        [DataMember]
        public string Domains { get; set; }

        [DataMember]
        public string LocalDate { get; set; }

        [DataMember]
        public string UserHostAddress { get; set; }
    }
}