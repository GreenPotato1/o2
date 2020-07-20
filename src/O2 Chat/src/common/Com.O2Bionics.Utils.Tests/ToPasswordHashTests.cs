using System;
using FluentAssertions;
using NUnit.Framework;

namespace Com.O2Bionics.Utils.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ToPasswordHashTests
    {
        [Test]
        public void TestNull()
        {
            Action action = () => ToPasswordHashExtensions.ToPasswordHash(null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void TestValue()
        {
            "text1".ToPasswordHash().Should().Be("text1".ToPasswordHash());
            "text1".ToPasswordHash().Should().NotBe("text2".ToPasswordHash());

            Console.WriteLine("text1".ToPasswordHash());
        }

        [Test]
        public void TestLength()
        {
            for (var i = 0; i < 300; i++)
                new string('*', i).ToPasswordHash().Length.Should().Be(40);
        }
    }
}