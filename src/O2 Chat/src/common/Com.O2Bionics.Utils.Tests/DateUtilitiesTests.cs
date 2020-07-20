using System;
using FluentAssertions;
using NUnit.Framework;

namespace Com.O2Bionics.Utils.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public sealed class DateUtilitiesTests
    {
        private readonly DateTime m_date = new DateTime(3789, 5, 31, 0, 0, 0, DateTimeKind.Utc);

        [Test]
        public void ParseDate()
        {
            var str = m_date.DateToString();
            Assert.AreEqual("37890531", str, "strings");

            var date2 = DateUtilities.ParseDate(str);
            Assert.AreEqual(m_date, date2, "dates");
        }

        [Test]
        public void ToFromDays()
        {
            var days = m_date.ToDays();
            Assert.AreEqual(m_date.Year * 366L + 3 * 31 + 28 + 30, days, "first days");

            var date2 = days.FromDays();
            Assert.AreEqual(m_date, date2, "dates");
        }

        private static readonly ValueTuple<string, DateTime>[] m_fromIso8601UtcDateTimeStringCases =
            {
                ("2018-03-27T07:20:54Z", new DateTime(2018, 3, 27, 7, 20, 54, DateTimeKind.Utc)),
                ("2018-03-27T07:20:54.123Z", new DateTime(2018, 3, 27, 7, 20, 54, DateTimeKind.Utc).AddMilliseconds(123)),
                ("2018-03-27T07:20:54.0664600Z", new DateTime(2018, 3, 27, 7, 20, 54, DateTimeKind.Utc).AddMilliseconds(66.46)),
            };

        [Test]
        public void TestFromIso8601UtcDateTimeString(
            [ValueSource(nameof(m_fromIso8601UtcDateTimeStringCases))]
            ValueTuple<string, DateTime> testCase)
        {
            (string s, DateTime expected) = testCase;

            var actual = DateUtilities.FromIso8601UtcDateTimeString(s);
            actual.Should().BeCloseTo(expected, 1);
            actual.Kind.Should().Be(DateTimeKind.Utc);
        }
    }
}