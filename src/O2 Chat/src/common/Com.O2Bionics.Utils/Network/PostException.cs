using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils.Network
{
    [Serializable]
    public sealed class PostException : Exception
    {
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp

        private const string HttpCodeName = "httpCode";

        [UsedImplicitly]
        public PostException()
        {
        }

        public PostException(int httpCode, [NotNull] string message, [CanBeNull] Exception inner = null)
            : base(message, inner)
        {
            HttpCode = httpCode;
        }

        private PostException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            HttpCode = info.GetInt32(HttpCodeName);
        }

        public int HttpCode { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(HttpCodeName, HttpCode);
        }

        public override string ToString()
        {
            var result = $"HTTP code={HttpCode}. {base.ToString()}";
            return result;
        }
    }
}