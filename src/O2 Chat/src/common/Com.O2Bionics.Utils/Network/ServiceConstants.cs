namespace Com.O2Bionics.Utils.Network
{
    public static class ServiceConstants
    {
        public const string Namespace = "http://o2bionics.com";
        public const string NamespaceShort = "o2bionics";

        public const string CustomerId = "CustomerId";
        public const string UserId = "UserId";
        public const string VisitorId = "VisitorId";

        public const string ClientIp = "ClientIp";
        public const string Url = "Url";
        public const string UserAgent = "UserAgent";

        // https://tools.ietf.org/pdf/rfc2181.pdf
        // A full domain name is limited to 255 octets (including the separators)
        public const int MaximumDomainLength = 255;
    }
}