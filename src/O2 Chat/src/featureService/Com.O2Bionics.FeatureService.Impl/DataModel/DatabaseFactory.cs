using System;
using System.Collections.Generic;
using System.Data;
using Unity.Attributes;

namespace Com.O2Bionics.FeatureService.Impl.DataModel
{
    public class DatabaseFactory : IDatabaseFactory
    {
        private const IsolationLevel IsolationLevel = System.Data.IsolationLevel.ReadCommitted;

        private readonly IReadOnlyDictionary<string, string> m_connectionStrings;

        [InjectionConstructor]
        public DatabaseFactory(FeatureServiceSettings settings)
        {
            LogEnabled = settings.LogSqlQuery;
            m_connectionStrings = settings.Databases;
        }

        public DatabaseFactory(string productCode, string connectionString, bool logSqlQuery)
        {
            LogEnabled = logSqlQuery;
            m_connectionStrings = new Dictionary<string, string> { { productCode, connectionString } };
        }

        public bool LogEnabled { get; set; }

        private Database Create(string productCode, bool logEnabled)
        {
            string connectionString;
            if (!m_connectionStrings.TryGetValue(productCode, out connectionString))
                throw new ProductCodeNotFoundException(string.Format("Can't find connection string for product code '{0}'", productCode));
            return new Database(connectionString, logEnabled);
        }

        public IReadOnlyList<string> ProductCodes
        {
            get { return new List<string>(m_connectionStrings.Keys); }
        }

        public T Query<T>(string productCode, Func<Database, T> func, bool? logEnabled = null)
        {
            using (var db = Create(productCode, logEnabled ?? LogEnabled))
            {
                db.BeginTransaction(IsolationLevel);
                try
                {
                    var result = func(db);
                    db.CommitTransaction();
                    return result;
                }
                catch
                {
                    db.RollbackTransaction();
                    throw;
                }
            }
        }

        public void Query(string productCode, Action<Database> action, bool? logEnabled = null)
        {
            Query(
                productCode,
                db =>
                    {
                        action(db);
                        return 0;
                    },
                logEnabled);
        }
    }
}