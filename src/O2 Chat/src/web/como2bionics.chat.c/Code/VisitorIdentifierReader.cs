using System.Collections.Specialized;
using System.Web;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.ChatService.Web.Chat
{
    /// <summary>
    /// Read CustomerId and VisitorId.
    /// </summary>
    public sealed class VisitorIdentifierReader : IIdentifierReader
    {
        public (uint customerId, uint userId, ulong visitorId) Read()
        {
            var queryString = HttpContext.Current?.Request.QueryString;
            if (null == queryString)
                return (0, 0, 0);
            var customerId = queryString.GetUint("cid");
            var visitorId = GetULong(queryString, "vid");
            return (customerId, 0, visitorId);
        }

        private static ulong GetULong(NameValueCollection queryString, params string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                var value = queryString[keys[i]];
                if (ulong.TryParse(value, out var result) && 0 < result)
                    return result;
               
            }
            return 0;
        }
    }
}