using System;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    public static class DateUtilities
    {
        private const long YearDays = 366;
        private const string DateFormat = "yyyyMMdd";
        private static readonly DateTime m_year1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Example of input: "20171128".
        /// </summary>
        public static DateTime ParseDate([NotNull] string date)
        {
            var result = DateTime.ParseExact(
                date,
                DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            return result;
        }

        public static DateTime FromUnixTimestamp(this double timestamp)
        {
            var result = m_year1970.AddSeconds(timestamp);
            return result;
        }

        public static double ToUnixTimestamp(this DateTime date)
        {
            var diff = date.ToUniversalTime() - m_year1970;
            return diff.TotalSeconds;
        }

        public static double ToUnixTimeMilliseconds(this DateTime date)
        {
            return new DateTimeOffset(date).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Return value sample: "2018-01-05T09:40:57.821Z".
        /// </summary>
        public static string ToUtcString(this DateTime date)
        {
            var result = date.ToString("u");
            return result;
        }

        private static readonly string[] m_iso8601Formats =
            {
                "o",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss.fffZ",
            };

        public static DateTime FromIso8601UtcDateTimeString([NotNull] string value)
        {
            if (!DateTime.TryParseExact(
                value,
                m_iso8601Formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var result))
                throw new ArgumentOutOfRangeException($"The string '{value}' cannot be parsed as Iso8601 UTC DateTime.");
            return result;
        }

        public static DateTime RemoveTime(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        public static DateTime RemoveMilliseconds(this DateTime dateTime)
        {
            var result = dateTime - new TimeSpan(0, 0, 0, 0, dateTime.TimeOfDay.Milliseconds);
            return result;
        }

        [NotNull]
        public static string DateToString(this DateTime date)
        {
            var result = date.ToString(DateFormat);
            return result;
        }

        public static long ToDays(this DateTime date)
        {
            var result = YearDays * date.Year + date.DayOfYear;
            Debug.Assert(0 <= result);
            return result;
        }

        public static DateTime FromDays(this long days)
        {
            Debug.Assert(0 <= days);
            checked
            {
                long year = days / YearDays, day = days % YearDays;
                var result = new DateTime((int)year, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(day - 1);
                return result;
            }
        }
    }
}