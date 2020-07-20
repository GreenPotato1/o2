using System;
using FluentAssertions;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;

namespace Com.O2Bionics.Utils.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class StringEncryptorTests
    {
        private const string Key = "asdfasd234jkytr_+sdfSDGfghjkytre";

        [Test]
        public void TestEncrypt([Values("", "   ", "1", "14324532543|test12")] string text)
        {
            var encrypted = StringEncryptor.Encrypt(text, Key);
            Console.WriteLine(encrypted);
            var decrypted = StringEncryptor.Decrypt(encrypted, Key);
            Console.WriteLine(decrypted);
            decrypted.Should().Be(text);
        }

        [Test]
        public void TestEncryptNull()
        {
            Action action = () => StringEncryptor.Encrypt(null, Key);
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void TestEncryptInvalidKey([Values(null, "", "  ")] string key)
        {
            Action action = () => StringEncryptor.Encrypt("A", key);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void TestEncryptKeyLength(
            [Values("lessthan128bit", "more then 256 bit AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAa")] string key)
        {
            const string text = "A";
            var encrypted = StringEncryptor.Encrypt(text, key);
            Console.WriteLine(encrypted);
            var decrypted = StringEncryptor.Decrypt(encrypted, key);
            Console.WriteLine(decrypted);
            decrypted.Should().Be(text);
        }

        [Test]
        public void TestDecryptNull()
        {
            Action action = () => StringEncryptor.Decrypt(null, Key);
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void TestDecryptInvalidCipher()
        {
            Action action = () => StringEncryptor.Decrypt(Convert.ToBase64String(new byte[] { 1, 2, 3 }), Key);
            action.Should().Throw<CryptoException>();
        }

        [Test]
        public void TestDecryptInvalidCipherNotBase64String()
        {
            Action action = () => StringEncryptor.Decrypt("!@#$", Key);
            action.Should().Throw<FormatException>();
        }

        [Test]
        public void TestDecryptEmptyOrWhitespaceCipher([Values("", "   ")] string cipher)
        {
            var text = StringEncryptor.Decrypt(cipher, Key);
            Console.WriteLine("'{0}'", text);
        }

        [Test]
        public void TestDecryptInvalidKey([Values(null, "", "  ")] string key)
        {
            Action action = () => StringEncryptor.Decrypt("eX2LMUlm9kQXKs88cVVWDw==", key);
            action.Should().Throw<ArgumentException>();
        }
    }
}