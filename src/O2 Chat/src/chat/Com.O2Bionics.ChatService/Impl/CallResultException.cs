using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Impl
{
    [Serializable]
    public class CallResultException : Exception
    {
        public CallResultStatus Status { get; }

        public CallResultException(CallResultStatusCode code, params ValidationMessage[] messages)
            : base(ToMessage(code, messages))
        {
            Status = new CallResultStatus(code, messages);
        }

        public CallResultException(CallResultStatusCode code, List<ValidationMessage> messages)
            : base(ToMessage(code, messages))
        {
            Status = new CallResultStatus(code, messages);
        }

        private static string ToMessage(CallResultStatusCode code, IEnumerable<ValidationMessage> messages)
        {
            return code.ToString("G")
                   + " [" + string.Join(", ", (messages ?? Array.Empty<ValidationMessage>()).Select(x => $"{x?.Field}: '{x?.Message}'")) + "]";
        }

        protected CallResultException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Status = (CallResultStatus)info.GetValue("status", typeof(CallResultStatus));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("status", Status, typeof(CallResultStatus));
            base.GetObjectData(info, context);
        }
    }
}