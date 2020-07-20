using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.PageTracker.Properties;
using log4net;

namespace Com.O2Bionics.PageTracker.DataModel
{
    public class DatabaseManager
    {
        protected readonly ILog Log;
        protected readonly string ConnectionString;
        protected readonly bool LogQueries;

        public DatabaseManager(string connectionString, bool logQueries = true)
        {
            ConnectionString = connectionString;
            LogQueries = logQueries;
            Log = LogManager.GetLogger(typeof(DatabaseManager));
        }

        protected string[] Tables { get; } =
            {
                "ip_country",
                "ip_city",
                "ip_address",
                "timezone",
                "user_agent",
                "site",
                "visitor",
                "page_history",
            };

        protected string SchemaScript { get; } = Resources.database;
        protected string ProceduresScript { get; } = Resources.add_record;


        protected void ExecuteInContext(Action<Database> action)
        {
            new DatabaseFactory(ConnectionString, LogQueries).Query(action);
        }

        protected void InsertInitialData(Database db, DateTime now)
        {
        }


        public void RecreateSchema()
        {
            Log.InfoFormat("Recreating schema in {0}{1}", Environment.NewLine, ConnectionString);
            using (var db = new Database(ConnectionString, LogQueries))
            {
                var statements = SchemaScriptStatements;
                foreach (var s in statements) db.Execute(s);

                db.Execute("DROP PROCEDURE IF EXISTS `add_record`");
                db.Execute(ProceduresScript);
            }
        }

        public virtual void DeleteData()
        {
            Log.InfoFormat("Removing data in {0}{1}", Environment.NewLine, ConnectionString);
            using (var db = new Database(ConnectionString, LogQueries))
            {
                foreach (var sql in Tables.Reverse().Select(x => $"delete from {x}"))
                {
                    try
                    {
                        db.Execute(sql);
                    }
                    catch (Exception e)
                    {
                        Log.WarnFormat("failed: " + e.Message);
                    }
                }
            }
        }

        public virtual void ReloadData()
        {
            DeleteData();

            var now = DateTime.UtcNow;

            Log.InfoFormat("Loading initial data in {0}{1}", Environment.NewLine, ConnectionString);

            ExecuteInContext(db => InsertInitialData(db, now));
        }

        protected List<string> SchemaScriptStatements
        {
            get
            {
                return SchemaScript.Split(';')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            }
        }
    }
}