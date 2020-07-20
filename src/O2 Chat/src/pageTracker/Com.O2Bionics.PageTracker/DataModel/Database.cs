using Com.O2Bionics.Utils;
using MySql.Data.MySqlClient;

namespace Com.O2Bionics.PageTracker.DataModel
{
    public class Database : DatabaseBase<MySqlConnection, MySqlTransaction, MySqlCommand, MySqlDataReader>
    {
        private const string ConnectionStringExtension =
            ";Character Set=utf8;Pooling=true;Min Pool Size=10;Max Pool Size=50;Persist Security Info=True;";

        public Database(string connectionString, bool log = false)
            : base(
                new MySqlConnection(connectionString + ConnectionStringExtension),
                log)
        {
        }

        protected override MySqlCommand CreateCommand(string text, MySqlConnection connection)
        {
            return new MySqlCommand(text, connection);
        }
    }
}