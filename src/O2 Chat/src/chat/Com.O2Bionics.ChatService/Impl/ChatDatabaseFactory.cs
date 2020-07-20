using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using Com.O2Bionics.ChatService.DataModel;
using log4net;
using LinqToDB.Data;
using Unity.Attributes;

namespace Com.O2Bionics.ChatService.Impl
{
    public class ChatDatabaseFactory : IChatDatabaseFactory
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(ChatDatabaseFactory));

        private readonly string m_connectionString;

        [InjectionConstructor]
        public ChatDatabaseFactory(ChatServiceSettings settings)
        {
            m_connectionString = settings.Database;
            EnableSqlLog(settings.LogSqlQuery);
        }

        public ChatDatabaseFactory(string connectionString, bool logSqlQuery = false)
        {
            m_connectionString = connectionString;
            EnableSqlLog(logSqlQuery);
        }

        private void EnableSqlLog(bool logSqlQuery)
        {
            if (logSqlQuery)
            {
                var log = LogManager.GetLogger(DataConnection.TraceSwitch.DisplayName);
                DataConnection.TurnTraceSwitchOn();
                DataConnection.OnTrace = info => LogSqlQuery(log, info);
            }
            else
            {
                DataConnection.TurnTraceSwitchOn(TraceLevel.Off);
            }
        }

        private static void LogSqlQuery(ILog log, TraceInfo info)
        {
            if (info.BeforeExecute) return;

            var sb = new StringBuilder();
            sb
                .Append("Command: ")
                .Append(info.SqlText)
                .AppendLine()
                .Append("Execution time: ")
                .Append(info.ExecutionTime)
                .AppendLine();
            if (info.RecordsAffected.HasValue)
                sb
                    .Append("Records Affected: ")
                    .Append(info.RecordsAffected.Value)
                    .AppendLine();
            if (info.Exception != null)
            {
                for (var exception = info.Exception; exception != null; exception = exception.InnerException)
                    sb
                        .AppendLine()
                        .AppendFormat("Exception: {0}", exception.GetType())
                        .AppendLine()
                        .AppendFormat("Message  : {0}", exception.Message)
                        .AppendLine()
                        .AppendLine(exception.StackTrace);
            }

            if (info.Exception != null)
                log.Error(sb.ToString());
            else
                log.Info(sb.ToString());
        }

        private ChatDatabase Create()
        {
            // "User ID=chat_dev;Password=chat_dev_100;Data Source=//db1.o2bionics.com:1521/ora11;"
            return new ChatDatabase(m_connectionString);
        }

        public T Query<T>(Func<ChatDatabase, T> func)
        {
            using (var db = Create())
            {
                db.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    var result = func(db);
                    db.CommitTransaction();
                    foreach (var action in db.OnCommitActions)
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception e)
                        {
                            m_log.Error("Exception in the OnCommitAction", e);
                        }
                    }

                    return result;
                }
                catch
                {
                    db.RollbackTransaction();
                    throw;
                }
            }
        }

        public void Query(Action<ChatDatabase> action)
        {
            Query(
                db =>
                    {
                        action(db);
                        return 0;
                    });
        }

        public IDataContext CreateContext()
        {
            return new DataContext(m_connectionString);
        }
    }
}