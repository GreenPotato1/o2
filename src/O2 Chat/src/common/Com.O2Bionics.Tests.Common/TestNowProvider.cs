using System;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.Tests.Common
{
    public sealed class TestNowProvider : INowProvider
    {
        private readonly Func<DateTime> m_func;

        public TestNowProvider() : this(UtcNowWithoutMilliseconds())
        {
        }

        public TestNowProvider(DateTime utcNow) : this(() => utcNow)
        {
        }

        public TestNowProvider([NotNull] Func<DateTime> func)
        {
            m_func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public DateTime UtcNow => m_func();

        // 
        // this solves problems with tests which compare json serialized dates.
        // 
        // jil doesn't trim trailing zeros in ISO8601 fractional seconds but newtonsoft does.
        // it looks like:
        //                               636479820918656110L
        //      -> (newtonsoft) 2017-12-04T11:01:31.865611Z
        //      -> (jil)        2017-12-04T11:01:31.8656110Z
        //
        // Current solution is to cut off the milliseconds.
        //
        // see also JsonTests.TestDateTimeFormatting()
        //
        public static DateTime UtcNowWithoutMilliseconds()
        {
            var now = DateTime.UtcNow;
            var result = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc);
            return result;
        }
    }
}