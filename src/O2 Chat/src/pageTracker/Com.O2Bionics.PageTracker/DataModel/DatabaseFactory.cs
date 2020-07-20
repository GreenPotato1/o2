using System;
using System.Data;
using log4net;
using MySql.Data.MySqlClient;
using Unity.Attributes;

namespace Com.O2Bionics.PageTracker.DataModel
{
    public class DatabaseFactory : IDatabaseFactory
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(DatabaseFactory));

        private const IsolationLevel IsolationLevel = System.Data.IsolationLevel.ReadCommitted;

        private readonly string m_connectionString;

        [InjectionConstructor]
        public DatabaseFactory(PageTrackerSettings settings)
        {
            LogEnabled = settings.LogSqlQuery;
            m_connectionString = settings.Database;
        }

        public DatabaseFactory(string connectionString, bool logEnabled)
        {
            LogEnabled = logEnabled;
            m_connectionString = connectionString;
        }

        public bool LogEnabled { get; set; }

        private Database Create(bool logEnabled)
        {
            return new Database(m_connectionString, logEnabled);
        }

        public T Query<T>(Func<Database, T> func, bool? logEnabled = null)
        {
            using (var db = Create(logEnabled ?? LogEnabled))
            {
                while (true)
                {
                    try
                    {
                        db.BeginTransaction(IsolationLevel);
                        try
                        {
                            var result = func(db);
                            db.CommitTransaction();
                            return result;
                        }
                        catch (Exception)
                        {
//                            m_log.WarnFormat("exception: " + e);
                            db.RollbackTransaction();
                            throw;
                        }
                    }
                    catch (MySqlException e)
                    {
                        if (e.Number == 1062)
                        {
                            m_log.WarnFormat("Duplicate key {0}", e);
                            continue;
                        }

                        throw;
                    }
                }
            }
        }

        public void Query(Action<Database> action, bool? logEnabled = null)
        {
            Query(
                db =>
                    {
                        action(db);
                        return 0;
                    },
                logEnabled);
        }
    }
}