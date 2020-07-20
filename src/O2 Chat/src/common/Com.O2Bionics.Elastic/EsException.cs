using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Com.O2Bionics.Elastic
{
    [Serializable]
    public class EsException : Exception
    {
        [UsedImplicitly]
        public EsException()
        {
        }

        public EsException(string message) : base(message)
        {
        }

        public EsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}