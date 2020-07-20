using System.Security.Claims;
using System.Web;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils.Web;

namespace Com.O2Bionics.ChatService.Web.Console
{
    /// <summary>
    /// Read CustomerId and UserId.
    /// </summary>
    public sealed class UserIdentifierReader : IIdentifierReader
    {
        public (uint customerId, uint userId, ulong visitorId) Read()
        {
            if (!(HttpContext.Current?.User?.Identity is ClaimsIdentity identity))
                return (0, 0, 0);

            const bool throwException = false;
            var customerId = identity.GetCustomerId(throwException);

            var userId = identity.GetUserId(throwException);

            return (customerId, userId, 0);
        }
    }
}