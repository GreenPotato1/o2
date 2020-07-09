using System;
using NUnit.Framework;
using O2.Black.Toolkit.Core;

namespace Tests.O2.Black.Toolkit.Core
{
    [TestFixture]
    public class UnixDateExtensions
    {
        private static readonly long LongDateTime = 1582318800;

        [Test]
        public void UnixDateExtensions_DateTimeToLong()
        {
            var date = new DateTime(2020, 02, 22, 00,0,0);
            var longResult = date.ConvertToUnixTime();
            Assert.AreEqual(LongDateTime, longResult);
        }

        [Test]
        public void UnixDateExtensions_LongToDateTime()
        {
            var dateTime = new DateTime(2020, 02, 22, 00,0,0);
            var utcDateTime = dateTime;
            Assert.AreEqual(LongDateTime.ConvertToDateTime().Ticks, utcDateTime.Ticks);
        }
    }
}