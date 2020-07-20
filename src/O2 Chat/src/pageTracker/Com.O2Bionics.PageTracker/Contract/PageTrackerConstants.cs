namespace Com.O2Bionics.PageTracker.Contract
{
    public static class PageTrackerConstants
    {
        // web api query string parameters
        public const string CustomerIdParameterName = "cid";
        public const string HistoryIdParameterName = "hid";
        public const string VisitorIdParameterName = "vid";
        public const string SearchPositionParameterName = "sp";

        public const string PageSizeParameterName = "sz";
        public const int PageHistoryGetLimitMax = 10 * 1000;

        public const string AddCommand = "a";
        public const string GetCommand = "g";

        public const int PageHistoryGetLimit = 20;

        public const int VisitorExternalIdMaxLength = 256;
        public const int CustomTextMaxLength = 2048;
        public const int TimeZoneDescriptionMaxLength = 128;
        public const int UserAgentStringMaxLength = 2048;
    }
}