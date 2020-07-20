using System;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    [Serializable]
    public class ValidationMessage
    {
        public ValidationMessage()
        {
        }

        public ValidationMessage(string field, string message)
        {
            Field = field;
            Message = message;
        }

        [DataMember]
        public string Field { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}