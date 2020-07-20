using System.Data;
using Com.O2Bionics.ChatService.DataModel;

namespace Com.O2Bionics.ChatService.Impl
{
    public class DataContext : IDataContext
    {
        private readonly string m_connectionString;
        private ChatDatabase m_database;

        public DataContext(string connectionString)
        {
            m_connectionString = connectionString;
        }

        public ChatDatabase Db
        {
            get
            {
                if (m_database == null)
                    m_database = new ChatDatabase(m_connectionString);
                if (m_database.Transaction == null)
                    m_database.BeginTransaction(IsolationLevel.ReadCommitted);
                return m_database;
            }
        }

        public void Commit()
        {
            if (m_database?.Transaction != null)
                m_database.CommitTransaction();
        }

        public void Rollback()
        {
            if (m_database?.Transaction != null)
                m_database.RollbackTransaction();
        }

        public void Dispose()
        {
            Rollback();
            if (m_database != null)
            {
                m_database.Dispose();
                m_database = null;
            }
        }
    }
}