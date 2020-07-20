using System;
using System.Security.Cryptography;

namespace Com.O2Bionics.Utils
{
    public static class EntityCreateUtility
    {
        public static readonly byte InsertAttemptsLimit = 3;

        private static readonly RNGCryptoServiceProvider m_generator = new RNGCryptoServiceProvider();

        public static uint GenerateId()
        {
            var buffer = new byte[sizeof(uint)];
            m_generator.GetBytes(buffer); // the method is thread safe
            return BitConverter.ToUInt32(buffer, 0);
        }
    }
}