using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using log4net;
using LinqToDB;
using LinqToDB.Mapping;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.Utils
{
    public abstract class DatabaseManagerBase<TDataContext>
    {
        //Procedures can have many semicolons (;) inside - have to separate by the ProcedurePrefix.
        private const string ProcedurePrefix = "CREATE PROCEDURE";

        protected readonly ILog Log;
        protected readonly string ConnectionString;
        protected readonly bool LogQueries;

        protected DatabaseManagerBase(string connectionString, bool logQueries, ILog logger)
        {
            ConnectionString = connectionString;
            LogQueries = logQueries;
            Log = logger;
        }

        protected abstract string SchemaScript { get; }
        protected virtual string ProceduresScript => string.Empty;
        protected abstract string[] Tables { get; }

        protected abstract void InsertInitialData(TDataContext db, DateTime now);


        public void RecreateSchema()
        {
            Log.InfoFormat("Recreating schema in {0}{1}", Environment.NewLine, ConnectionString);
            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                var statements = SchemaScriptStatements;
                DropObjects(conn, statements);

                var procedures = SchemaScriptProcedures;
                DropObjectsOfType(conn, procedures, "PROCEDURE", 4043);

                {
                    var names = GetTables(conn);
                    if (names.Any())
                        Log.WarnFormat("Unsupported tables in the db: {0}", string.Join(",", names));
                }
                {
                    var names = GetProcedures(conn);
                    if (names.Any())
                        Log.WarnFormat("Unsupported procedures in the db: {0}", string.Join(",", names));
                }

                foreach (var s in statements)
                    Execute(conn, s);
                foreach (var s in procedures)
                    Execute(conn, s);
            }
        }

        public virtual void DeleteData()
        {
            Log.InfoFormat("Removing data in {0}{1}", Environment.NewLine, ConnectionString);
            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                var sql = "begin"
                          + Environment.NewLine
                          + string.Join(Environment.NewLine, Tables.Reverse().Select(x => $"delete from {x};"))
                          + Environment.NewLine
                          + "end;";
                Execute(conn, sql);
            }
        }

        protected abstract void ExecuteInContext(Action<TDataContext> action);

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

        protected List<string> SchemaScriptProcedures
        {
            get
            {
                return ProceduresScript.Split(new[] { ProcedurePrefix }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => ProcedurePrefix + ' ' + x)
                    .ToList();
            }
        }

        protected List<string> GetTables(OracleConnection conn)
        {
            var user = new OracleConnectionStringBuilder(ConnectionString).UserID.ToLowerInvariant();
            var sql = $"SELECT table_name FROM dba_tables where lower(owner) = '{user}' order by table_name";
            return ExecuteReader(conn, sql, r => r.GetString(0));
        }

        protected List<string> GetProcedures(OracleConnection conn)
        {
            var user = new OracleConnectionStringBuilder(ConnectionString).UserID.ToLowerInvariant();
            var sql = $@"SELECT OBJECT_NAME FROM dba_procedures 
where lower(owner) = '{user}' and OBJECT_NAME is not null and 0 < LENGTH(OBJECT_NAME) 
order by OBJECT_NAME";
            return ExecuteReader(conn, sql, r => r.GetString(0));
        }

        protected void DropObjects(OracleConnection db, IReadOnlyCollection<string> script)
        {
            DropObjectsOfType(db, script, "SEQUENCE", 2289);
            DropObjectsOfType(db, script, "INDEX", 1418);
            DropObjectsOfType(db, script, "TABLE", 942);
        }

        private void DropObjectsOfType(
            OracleConnection db,
            IEnumerable<string> script,
            string objectType,
            int notExistsErrorNumber)
        {
            var names = GetNamesOfType(script, objectType);
            foreach (var x in names)
            {
                try
                {
                    var m = objectType.Equals("table", StringComparison.OrdinalIgnoreCase)
                        ? " cascade constraints"
                        : "";
                    Execute(db, $"DROP {objectType} {x} {m}");
                }
                catch (OracleException e)
                {
                    if (e.Number == notExistsErrorNumber)
                        Log.WarnFormat("{0} {1} doesn't exists", objectType, x);
                    else
                        throw;
                }
            }
        }

        private static List<string> GetNamesOfType(IEnumerable<string> script, string objectType)
        {
            var createObjectRe = new Regex(
                $@"\s*CREATE\s+{objectType}\s*(\w+)\s*.*",
                RegexOptions.IgnoreCase);
            return script
                .Select(x => createObjectRe.Match(x))
                .Where(x => x.Success)
                .Select(x => x.Groups[1].Value)
                .Reverse()
                .ToList();
        }

        protected static void Insert<T>(IDataContext db, params T[] objects)
        {
            var identity =
                typeof(T).GetProperties().FirstOrDefault(x => x.GetCustomAttribute<IdentityAttribute>() != null);
            if (identity != null)
            {
                foreach (var obj in objects)
                {
                    var id = (decimal)db.InsertWithIdentity(obj);
                    identity.SetValue(obj, id);
                }
            }
            else
            {
                foreach (var obj in objects) db.Insert(obj);
            }
        }

        private List<T> ExecuteReader<T>(OracleConnection conn, string sql, Func<OracleDataReader, T> read)
        {
            var list = new List<T>();
            using (var cmd = new OracleCommand(sql, conn))
            {
                if (LogQueries) Log.DebugFormat("reader:{0}{1}", Environment.NewLine, sql);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        list.Add(read(reader));
                }
            }

            return list;
        }

        protected void Execute(OracleConnection conn, string sql)
        {
            using (var cmd = new OracleCommand(sql, conn))
            {
                if (LogQueries) Log.DebugFormat("{0}{1}", Environment.NewLine, sql);
                cmd.ExecuteNonQuery();
            }
        }
    }
}