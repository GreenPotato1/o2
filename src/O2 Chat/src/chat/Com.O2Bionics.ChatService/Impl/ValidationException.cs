using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Impl
{
    [Serializable]
    public class ValidationException : Exception
    {
        public List<ValidationMessage> Messages { get; }

        public ValidationException(ValidationMessage message)
            : base(ToMessage(new[] { message }))
        {
            Messages = new List<ValidationMessage> { message };
        }

        public ValidationException(List<ValidationMessage> messages)
            : base(ToMessage(messages))
        {
            Messages = messages;
        }

        private static string ToMessage(IEnumerable<ValidationMessage> messages)
        {
            return "[" + string.Join(", ", (messages ?? Array.Empty<ValidationMessage>()).Select(x => $"{x?.Field}: '{x?.Message}'")) + "]";
        }

        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Messages = (List<ValidationMessage>)info.GetValue("messages", typeof(List<ValidationMessage>));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("messages", Messages, typeof(List<ValidationMessage>));
            base.GetObjectData(info, context);
        }
    }
}