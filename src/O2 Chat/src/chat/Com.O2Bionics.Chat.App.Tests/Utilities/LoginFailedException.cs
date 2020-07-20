using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Com.O2Bionics.Chat.App.Tests.Utilities
{
    [Serializable]
    public sealed class LoginFailedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        [UsedImplicitly]
        public LoginFailedException()
        {
        }

        public LoginFailedException(string message) : base(message)
        {
        }

        public LoginFailedException(string message, Exception inner) : base(message, inner)
        {
        }

        [UsedImplicitly]
        private LoginFailedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}