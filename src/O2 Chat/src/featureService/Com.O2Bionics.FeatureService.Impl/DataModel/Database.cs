using Com.O2Bionics.Utils;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.FeatureService.Impl.DataModel
{
    public class Database : DatabaseBase<OracleConnection, OracleTransaction, OracleCommand, OracleDataReader>
    {
        public Database(string connectionString, bool log = false)
            : base(new OracleConnection(connectionString), log)
        {
        }

        protected override OracleCommand CreateCommand(string text, OracleConnection connection)
        {
            return new OracleCommand(text, connection);
        }
    }
}