using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Com.O2Bionics.Elastic
{
    [Serializable]
    public sealed class NotFoundElasticDocumentException : Exception
    {
        [UsedImplicitly]
        public NotFoundElasticDocumentException()
        {
        }

        public NotFoundElasticDocumentException(string message) : base(message)
        {
        }

        public NotFoundElasticDocumentException(string message, Exception inner) : base(message, inner)
        {
        }

        private NotFoundElasticDocumentException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}