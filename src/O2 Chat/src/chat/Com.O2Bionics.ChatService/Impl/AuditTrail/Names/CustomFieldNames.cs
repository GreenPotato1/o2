using Com.O2Bionics.Utils.Network;

namespace Com.O2Bionics.ChatService.Impl.AuditTrail.Names
{
    /// <summary>
    ///     Keys of "AuditEvent{T}.CustomValues".
    /// </summary>
    public static class CustomFieldNames
    {
        //All strings must be unique.

        //Note. User Id (UserId) is stored in the "Author.Id" field.

        public const string VisitorId = ServiceConstants.VisitorId;

        /// <summary>
        ///     IP address.
        /// </summary>
        public const string ClientIp = ServiceConstants.ClientIp;

        public const string ExceptionMessage = "ExceptionMessage";

        public const string ClientType = "ClientType";
        public const string ClientVersion = "ClientVersion";
        public const string ClientLocalDate = "ClientLocalDate";
    }
}