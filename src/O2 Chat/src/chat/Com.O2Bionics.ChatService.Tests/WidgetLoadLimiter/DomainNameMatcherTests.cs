using Com.O2Bionics.Utils;
using NUnit.Framework;
using TestCase = System.ValueTuple<string, string, bool>;

namespace Com.O2Bionics.ChatService.Tests.WidgetLoadLimiter
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public sealed class DomainNameMatcherTests
    {
        private static readonly TestCase[] m_testCases =
            {
                new TestCase("domain.com", "domain.com", true),
                new TestCase("Domain.Com;abc.def", "domain.com", true),
                new TestCase("DOMAIN.COM", "domain.com", true),
                new TestCase("com", "com", true),
                new TestCase("coM", "Com", true),
                //
                new TestCase("domain.com", "sub.DOMAIN.COM", true),
                new TestCase("sub.DOMAIN.COM", "domain.com", false),
                //
                new TestCase("com", "com1", false),
                new TestCase("com1", "com", false),
                //
                new TestCase("com", "1com", true),
                new TestCase("com", "A.com", true),
                new TestCase("com", "A.b.c.d.com", true),
                new TestCase("1com", "com", false),
                //
                new TestCase("domain.com", "domain;com", false),
                new TestCase("some.domain.com", "domain.com", false),
                new TestCase("some.domain.com", "d.some.domain.com", true),
                new TestCase("somedomain.com", "c.d.some.domaiN.COm", false),
                new TestCase("some.domain.com", "a.B.c.d.some.domain.coM", true),
                new TestCase("some1.domain.com", "a.B.c.d.some.domain.coM", false),
            };

        [Test]
        [TestCaseSource(nameof(m_testCases))]
        public void Test(TestCase testCase)
        {
            var actual = DomainUtilities.HasDomain(testCase.Item1, testCase.Item2);
            Assert.AreEqual(testCase.Item3, actual, testCase.ToString());
        }
    }
}