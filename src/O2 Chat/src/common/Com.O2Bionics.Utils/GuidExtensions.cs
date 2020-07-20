using System;
using log4net;

namespace Com.O2Bionics.Utils
{
    public static class GuidExtensions
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(GuidExtensions));

        public static string AsSignalRConnectionId(this Guid guid)
        {
            return guid.ToString("d");
        }

        public static string AsSqlString(this Guid guid)
        {
            return guid.ToString("N");
        }

        public static string AsLogString(this Guid guid)
        {
            return guid.ToString("D");
        }

        public static string AsWebString(this Guid guid)
        {
            return guid.ToString("N");
        }

        public static Guid FromWebGuid(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException("Can't be null or whitespace", "s");
            return Guid.ParseExact(s, "N");
        }

        public static Guid? FromOptionalWebGuid(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            Guid guid;
            return Guid.TryParseExact(s, "N", out guid) ? guid : (Guid?)null;
        }


        private static string AsShortString(this Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray())
                .Substring(0, 22)
                .Replace("/", "_")
                .Replace("+", "-");
        }

        private static Guid FromShortStringGuid(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException("Can't be null or whitespace", "s");
            return new Guid(Convert.FromBase64String(s.Replace("_", "/").Replace("-", "+") + "=="));
        }

        private static Guid? FromShortStringOptionalGuid(this string s)
        {
            return string.IsNullOrWhiteSpace(s) ? (Guid?)null : FromShortStringGuid(s);
        }
    }
}