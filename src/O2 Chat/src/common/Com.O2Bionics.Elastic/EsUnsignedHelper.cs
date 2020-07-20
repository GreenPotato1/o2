namespace Com.O2Bionics.Elastic
{
    // Elastic Search doesn't have unsigned data types support for now. 
    public static class EsUnsignedHelper
    {
        public static long ToEs(ulong value)
        {
            return (long)value;
        }

        public static ulong FromEs(long storeValue)
        {
            return (ulong)storeValue;
        }
    }
}