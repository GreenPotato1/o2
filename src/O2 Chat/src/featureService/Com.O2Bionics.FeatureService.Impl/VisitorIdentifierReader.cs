using System.Web;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.FeatureService.Impl
{
    /// <summary>
    /// Read CustomerId.
    /// </summary>
    public sealed class VisitorIdentifierReader : IIdentifierReader
    {
        public (uint customerId, uint userId, ulong visitorId) Read()
        {
            var queryString = HttpContext.Current?.Request.QueryString;
            if (null == queryString)
                return (0, 0, 0);

            var customerId = queryString.GetUint(ParameterNames.CustomerId);
            return (customerId, 0, 0);
        }
    }
}