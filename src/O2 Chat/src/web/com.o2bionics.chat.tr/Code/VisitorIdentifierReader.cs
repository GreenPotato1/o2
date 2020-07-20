using System.Web;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.ChatService.Web.PageTracker.Code
{
    /// <summary>
    /// Read CustomerId and VisitorId.
    /// </summary>
    public sealed class VisitorIdentifierReader : IIdentifierReader
    {
        public (uint customerId, uint userId, ulong visitorId) Read()
        {
            var queryString = HttpContext.Current?.Request.QueryString;
            var form = HttpContext.Current?.Request.Form;
            if (null == queryString && null == form)
                return (0, 0, 0);

            var collections = new[] { queryString, form };
            var customerId = collections.GetUint(PageTrackerConstants.CustomerIdParameterName);
            var visitorId = collections.GetULong(PageTrackerConstants.VisitorIdParameterName);
            return (customerId, 0, visitorId);
        }
    }
}