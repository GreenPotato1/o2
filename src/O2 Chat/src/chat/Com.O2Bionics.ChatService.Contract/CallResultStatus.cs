using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Jil;

namespace Com.O2Bionics.ChatService.Contract
{
    [DataContract]
    [Serializable]
    public class CallResultStatus
    {
        public CallResultStatus()
        {
        }

        public CallResultStatus(CallResultStatusCode statusCode, params ValidationMessage[] messages)
        {
            StatusCode = statusCode;
            Messages = messages?.ToList() ?? new List<ValidationMessage>();
        }

        public CallResultStatus(CallResultStatusCode statusCode, List<ValidationMessage> messages)
        {
            StatusCode = statusCode;
            Messages = messages ?? new List<ValidationMessage>();
        }

        [DataMember]
        [JilDirective(TreatEnumerationAs = typeof(int))]
        public CallResultStatusCode StatusCode { get; set; }

        [DataMember]
        public List<ValidationMessage> Messages { get; set; }
    }
}