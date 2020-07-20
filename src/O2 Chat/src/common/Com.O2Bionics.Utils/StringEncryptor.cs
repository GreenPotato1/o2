using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace Com.O2Bionics.Utils
{
    public static class StringEncryptor
    {
        private static readonly Encoding m_encoding = Encoding.ASCII;

        public static string Encrypt(string plain, string key)
        {
            if (plain == null) throw new ArgumentNullException("plain");

            var result = BouncyCastleCrypto(true, m_encoding.GetBytes(plain), key);
            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string cipher, string key)
        {
            if (cipher == null) throw new ArgumentNullException("cipher");

            var result = BouncyCastleCrypto(false, Convert.FromBase64String(cipher), key);
            return m_encoding.GetString(result);
        }

        private static byte[] BouncyCastleCrypto(bool isEncrypt, byte[] input, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Can't be null or whitespace", "key");

            // Key length should be 128/192/256 bits.
            const int requiredLength = 256 / 8;
            if (key.Length > requiredLength)
                key = key.Substring(0, requiredLength);
            else if (key.Length < requiredLength)
                key = key + new string('~', requiredLength - key.Length);

            try
            {
                var blockCipher = new AesEngine();
                var cipher = new PaddedBufferedBlockCipher(blockCipher);
                cipher.Init(isEncrypt, new KeyParameter(m_encoding.GetBytes(key)));
                return cipher.DoFinal(input);
            }
            catch (CryptoException ex)
            {
                throw new CryptoException(ex.Message);
            }
        }
    }
}