using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    public class CreateCustomerResult
    {
        public CreateCustomerResult(CallResultStatus callResultStatus, CustomerInfo customer = null)
        {
            Status = callResultStatus;
            Customer = customer;
        }

        [DataMember]
        public CallResultStatus Status { get; set; }

        [DataMember]
        public CustomerInfo Customer { get; set; }
    }
}