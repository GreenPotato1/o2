using System;
using System.Security.Cryptography;
using System.Text;

namespace Com.O2Bionics.Utils
{
    public static class O2Bionics3DesEncryptor
    {
        private const CipherMode CipherMode = System.Security.Cryptography.CipherMode.ECB;
        private const PaddingMode PaddingMode = System.Security.Cryptography.PaddingMode.Zeros;

        private static readonly Encoding m_encoding = Encoding.Unicode;

        public static byte[] Enrypt(string text, string key)
        {
            var keyBytes = Convert.FromBase64String(key);
            var dataBytes = m_encoding.GetBytes(text);
            using (var provider = new TripleDESCryptoServiceProvider
                {
                    Mode = CipherMode,
                    Padding = PaddingMode,
                    Key = keyBytes,
                })
            {
                using (var encryptor = provider.CreateEncryptor())
                {
                    return encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                }
            }
        }

        public static string Decrypt(byte[] data, string key)
        {
            var keyBytes = Convert.FromBase64String(key);
            using (var provider = new TripleDESCryptoServiceProvider
                {
                    Mode = CipherMode,
                    Padding = PaddingMode,
                    Key = keyBytes,
                })
            {
                using (var decryptor = provider.CreateDecryptor())
                {
                    var decrypted = decryptor.TransformFinalBlock(data, 0, data.Length);
                    return m_encoding.GetString(decrypted).Trim('\u0000');
                }
            }
        }
    }
}