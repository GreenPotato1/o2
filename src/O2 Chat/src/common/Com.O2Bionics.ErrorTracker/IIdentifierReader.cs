using JetBrains.Annotations;

namespace Com.O2Bionics.ErrorTracker
{
    public interface IIdentifierReader
    {
        /// <summary>
        /// Read the Customer Id, User Id, Visitor Id.
        /// </summary>
        [Pure]
        (uint customerId, uint userId, ulong visitorId) Read();
    }
}