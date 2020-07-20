namespace Com.O2Bionics.Utils
{
    public static class FieldConstants
    {
        public const string Keyword = "keyword";
        public const string KeywordSuffix = "." + Keyword;
        public const int IgnoreAbove = 32766;

        //public const string PreferredTypeName = "_doc";
        //As of Jan 2018, "_doc" should work in ES 6.1.1, but it does not.
        // https://www.elastic.co/guide/en/elasticsearch/reference/6.x/removal-of-types.html
        public const string PreferredTypeName = "doc";
    }
}