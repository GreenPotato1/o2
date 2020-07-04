using System;

namespace O2.Black.Toolkit.Core
{
    public static class UnixDateExtensions
    {
        private static readonly DateTime UnixEpoch =
            new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();

        public static DateTime ConvertToDateTime(this long seconds, bool isSeconds = true)
        {
            return isSeconds ? UnixEpoch.AddSeconds(seconds) : UnixEpoch.AddMinutes(seconds);
        }

        public static long ConvertToUnixTime(this DateTime datetime, bool isSeconds = true)
        {
            return isSeconds ? (long) (datetime  - UnixEpoch).TotalSeconds : (long) (datetime - UnixEpoch).TotalMinutes;
        }
    }
}