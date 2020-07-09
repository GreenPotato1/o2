using NUnit.Framework;
using System;
namespace Tests.O2.Black.Toolkit.Core
{
    [TestClass]
    public class UnixDateExtensionsTests
    {
        [Test]
        public void DateTimeToUnixDate()
        {
            DateTime testDate = new DateTime(2020,02,22);
            long longDate = testDate.ConvertToUnixTime();
            Assert.IsEqual(1582329600,longDate);
        }
    }
}