using Com.O2Bionics.ErrorTracker;

namespace Com.O2Bionics.MailerService.Web
{
    /// <summary>
    /// The credentials are not passed in.
    /// </summary>
    public sealed class FakeIdentifierReader : IIdentifierReader
    {
        public (uint customerId, uint userId, ulong visitorId) Read()
        {
            return (0, 0, 0);
        }
    }
}