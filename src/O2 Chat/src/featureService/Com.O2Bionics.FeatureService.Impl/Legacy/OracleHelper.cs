using System;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.FeatureService.Impl.Legacy
{
    public class OracleHelper
    {
        public static string GetStringNull(OracleDataReader reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return GetStringNull(reader, index);
        }

        public static string GetStringNull(OracleDataReader reader, int index)
        {
            return reader.GetString(index);
        }

        public static int DbIntValue(object value)
        {
            return Convert.ToInt32(value);
        }

        public static int GetInt32Null(OracleDataReader reader, string name)
        {
            var index = reader.GetOrdinal(name);
            return reader.IsDBNull(index) ? 0 : reader.GetInt32(index);
        }
    }
}