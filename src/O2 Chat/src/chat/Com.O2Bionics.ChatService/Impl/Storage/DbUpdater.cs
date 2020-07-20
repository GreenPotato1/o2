using System;
using System.Threading;
using log4net;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public class DbUpdater : IDbUpdater
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(DbUpdater));

        private readonly IChatDatabaseFactory m_databaseFactory;

        private readonly IVisitorStorage m_visitorStorage;
        private readonly IAgentSessionStorage m_agentSessionStorage;
        private readonly IChatSessionStorage m_chatSessionStorage;

        private Timer m_timer;

        public DbUpdater(
            IChatDatabaseFactory databaseFactory,
            IVisitorStorage visitorStorage,
            IAgentSessionStorage agentSessionStorage,
            IChatSessionStorage chatSessionStorage)
        {
            m_databaseFactory = databaseFactory;
            m_visitorStorage = visitorStorage;
            m_agentSessionStorage = agentSessionStorage;
            m_chatSessionStorage = chatSessionStorage;
        }

        public void Load()
        {
            m_databaseFactory.Query(
                db =>
                    {
                        m_visitorStorage.Load(db);
                        m_agentSessionStorage.Load(db);
                        m_chatSessionStorage.Load(db);
                    });
        }

        public void Start()
        {
            m_timer = new Timer(_ => Update(), null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(30));
        }

        public void Stop()
        {
            m_log.InfoFormat("stopping updater");

            m_timer.Dispose();
            m_timer = null;

            Thread.Sleep(TimeSpan.FromSeconds(5));
            m_log.InfoFormat("stopping updater: writing changes to database");
            Update();
        }

        public void Update()
        {
            UpdateDb(m_visitorStorage);
            UpdateDb(m_agentSessionStorage);
            UpdateDb(m_chatSessionStorage);
        }

        private void UpdateDb<T>(IDbUpdaterStorage<T> storage)
            where T : class
        {
            try
            {
                var updates = storage.GetDbUpdates();
                if (updates.Count == 0) return;

                m_log.DebugFormat("updating {0}", storage.GetType().Name);

                storage.UpdateDb(m_databaseFactory, updates);
                storage.ApplyDbUpdateResult(updates);
            }
            catch (Exception e)
            {
                m_log.Error($"Exception while synchronizing {storage.GetType().Name} to db.", e);
            }
        }
    }
}