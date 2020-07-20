using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    public class O2Bionics3DesEncryptTests
    {
        [Test]
        public void CreateKeyTest()
        {
            Console.WriteLine(Convert.ToBase64String(CreateKey()));
        }

        [Test]
        public void T([Values("", "a", "A", "aa", "aaa", "aaaa", "asdASD", "{id:3245,email:'some@text.email'}")] string s)
        {
            var key = CreateKey();
            WriteBytes("key", key);

            var e = Encrypt(s, key);
            WriteBytes("encrypted", e);

            using (var ms = new MemoryStream())
            {
                using (var z = new DeflateStream(ms, CompressionLevel.Optimal))
                    z.Write(e, 0, e.Length);
                WriteBytes("enc defl:", ms.GetBuffer());
            }

            var s2 = Decrypt(e, key);
            Console.WriteLine(s2);
        }

        private static void WriteBytes(string t, byte[] key)
        {
            Console.WriteLine(t + ":" + Environment.NewLine + string.Join(" ", key.Select(x => x.ToString("X2"))));
        }

        public static byte[] CreateKey()
        {
            using (var provider = new TripleDESCryptoServiceProvider
                {
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.Zeros,
                    KeySize = 128,
                })
            {
                provider.GenerateKey();
                return provider.Key;
            }
        }

        public static byte[] Encrypt(string data, byte[] key)
        {
            var dataBytes = Encoding.Unicode.GetBytes(data);
            WriteBytes("string", dataBytes);
            using (var provider = new TripleDESCryptoServiceProvider
                {
                    Key = key,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.Zeros,
                })
            {
                using (var encryptor = provider.CreateEncryptor())
                {
                    return encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                }
            }
        }

        public static string Decrypt(byte[] data, byte[] key)
        {
            using (var provider = new TripleDESCryptoServiceProvider
                {
                    Key = key,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.Zeros,
                })
            {
                using (var decryptor = provider.CreateDecryptor())
                {
                    var decryptedBytes = decryptor.TransformFinalBlock(data, 0, data.Length);
                    WriteBytes("decrypted", decryptedBytes);
                    return Encoding.Unicode.GetString(decryptedBytes).Trim('\u0000');
                }
            }
        }
    }
}