using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using log4net;

namespace Com.O2Bionics.Utils
{
    public abstract class DatabaseBase
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(DatabaseBase));
    }

    public abstract class DatabaseBase<TConnection, TTransaction, TCommand, TDataReader>
        : DatabaseBase, IDisposable
        where TConnection : DbConnection
        where TTransaction : DbTransaction
        where TCommand : DbCommand
        where TDataReader : DbDataReader
    {
        private readonly bool m_logEnabled;

        private TConnection m_conn;
        private TTransaction m_transaction;

        protected DatabaseBase(TConnection connection, bool logEnabled)
        {
            m_logEnabled = logEnabled;
            m_conn = connection;

            m_conn.Open();
            m_transaction = null;
        }

        protected abstract TCommand CreateCommand(string text, TConnection connection);

        public void Dispose()
        {
            if (m_transaction != null)
            {
                m_transaction.Rollback();
                m_transaction.Dispose();
                m_transaction = null;
            }

            m_conn.Close();
            m_conn.Dispose();
            m_conn = null;
        }

        public TDataReader ExecuteReader(TCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            cmd.Connection = m_conn;
            cmd.Transaction = m_transaction;
            LogCommand(cmd, "ExecuteReader");
            return (TDataReader)cmd.ExecuteReader();
        }

        public object ExecuteScalar(TCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            cmd.Connection = m_conn;
            cmd.Transaction = m_transaction;
            LogCommand(cmd, "ExecuteScalar");
            return cmd.ExecuteScalar();
        }

        public object ExecuteScalar(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("Can't be null or whitespace", "sql");

            using (var cmd = CreateCommand(sql, m_conn))
            {
                cmd.Connection = m_conn;
                cmd.Transaction = m_transaction;
                LogCommand(cmd, "ExecuteScalar");
                return cmd.ExecuteScalar();
            }
        }

        public int Execute(TCommand cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            cmd.Connection = m_conn;
            cmd.Transaction = m_transaction;
            LogCommand(cmd, "ExecuteNonQuery");
            return cmd.ExecuteNonQuery();
        }

        public int Execute(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("Can't be null or whitespace", "sql");

            using (var cmd = CreateCommand(sql, m_conn))
            {
                cmd.Transaction = m_transaction;
                LogCommand(cmd, "ExecuteNonQuery");
                return cmd.ExecuteNonQuery();
            }
        }

        private void LogCommand(IDbCommand cmd, string action)
        {
            if (!m_logEnabled) return;

            var pp = cmd.Parameters
                .Cast<IDbDataParameter>()
                .Select(p => string.Format("{1} {0} = {2}", p.ParameterName, p.DbType, p.Value == null ? "(null)" : p.Value.ToString()))
                .ToList();

            Log.DebugFormat(
                "{0}:{1}{3}{2}",
                action,
                Environment.NewLine,
                cmd.CommandText,
                pp.Any() ? string.Join(Environment.NewLine, pp) + Environment.NewLine : "");
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (m_transaction != null) throw new InvalidOperationException("Transaction is already created.");

            m_transaction = (TTransaction)m_conn.BeginTransaction(isolationLevel);
        }

        public void RollbackTransaction()
        {
            if (m_transaction == null) throw new InvalidOperationException("No active transactions found");

            m_transaction.Rollback();
            m_transaction.Dispose();
            m_transaction = null;
        }

        public void CommitTransaction()
        {
            if (m_transaction == null) throw new InvalidOperationException("No active transactions found");

            m_transaction.Commit();
            m_transaction.Dispose();
            m_transaction = null;
        }
    }
}