using Com.O2Bionics.Chat.App.Tests.Utilities;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Com.O2Bionics.Chat.App.Tests
{
    [TestFixture]
    public sealed class ValidateServerNameTest
    {
        private static void ValidateServerNames([NotNull] string[] values, bool expected)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < values.Length; i++)
            {
                var actual = NameValidator.ValidateServerName(values[i], false);
                Assert.AreEqual(expected, actual, values[i]);
            }
        }

        [Test]
        public void ValidateServerNameInvalid()
        {
            ValidateServerNames(new[] { "-", ".", ".a", "a.", "-c", "c-", "http://" }, false);
        }

        [Test]
        public void ValidateServerNameValid()
        {
            ValidateServerNames(new[] { "aa", "server1", "a.b", "a.b-c" }, true);
        }
    }
}