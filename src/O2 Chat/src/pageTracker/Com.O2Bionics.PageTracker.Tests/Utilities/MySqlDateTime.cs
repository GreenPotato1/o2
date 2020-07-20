using System;

namespace Com.O2Bionics.PageTracker.Tests.Utilities
{
    public static class MySqlDateTime
    {
        /// <summary>
        ///     Returns UtcNow() value rounded to seconds.
        /// </summary>
        /// <returns></returns>
        public static DateTime UtcNow()
        {
            return new DateTime(DateTime.UtcNow.Ticks / 10000000L * 10000000L);
        }
    }
}