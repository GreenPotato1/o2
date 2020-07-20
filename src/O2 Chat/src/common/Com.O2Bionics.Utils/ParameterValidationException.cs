using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    [Serializable]
    public sealed class ParameterValidationException : Exception
    {
        public ParameterValidationException()
        {
        }

        public ParameterValidationException(string message) : base(message)
        {
        }

        public ParameterValidationException(string message, Exception inner) : base(message, inner)
        {
        }

        public ParameterValidationException(string message, string value)
            : base(string.Format(message, value))
        {
        }

        [UsedImplicitly]
        private ParameterValidationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}