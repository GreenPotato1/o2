using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Settings;
using Com.O2Bionics.Utils;
using log4net;
using LinqToDB;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public class SettingsStorage : ISettingsStorage, IDisposable
    {
        private const string ChatSettingsPropertyFolder = "Chat";

        private static readonly ILog m_log = LogManager.GetLogger(typeof(SettingsStorage));

        public void Load(IDataContext dc)
        {
            PreloadServiceSettings(dc);
            PreloadCustomerSettings(dc);
        }

        #region ServiceSettings

        private readonly ReaderWriterLockSlim m_serviceSettingsLock = new ReaderWriterLockSlim();
        private ServiceSettings m_serviceSettings;

        private void PreloadServiceSettings(IDataContext dc)
        {
            m_log.Debug("loading service settings");
            var records = GetServiceSettingsRecords(dc).ToList();
            m_serviceSettings = new ServiceSettings(records);
        }

        private IQueryable<PROPERTY_BAG> GetServiceSettingsRecords(IDataContext dc)
        {
            return from p in dc.Db.PROPERTY_BAG
                where p.PROPERTY_FOLDER == ChatSettingsPropertyFolder
                      && p.CUSTOMER_ID == null && p.USER_ID == null
                select p;
        }

        public ServiceSettings GetServiceSettings()
        {
            return m_serviceSettingsLock.Read(() => m_serviceSettings);
        }

        public WritableServiceSettings GetWritableServiceSettings()
        {
            return new WritableServiceSettings(GetServiceSettings());
        }

        public void SaveServiceSettings(IDataContext dc, WritableServiceSettings settings)
        {
            m_serviceSettingsLock.Write(
                () =>
                    {
                        SaveSettingsRecords(
                            dc,
                            settings.GetDirtyRecords(),
                            x =>
                                {
                                    x.CUSTOMER_ID = null;
                                    x.USER_ID = null;
                                });
                        m_serviceSettings = new ServiceSettings(settings);
                    });
        }

        #endregion

        #region CustomerSettings

        private class Protected<T> : IDisposable
        {
            public Protected(T item)
            {
                Lock = new ReaderWriterLockSlim();
                Item = item;
            }

            public ReaderWriterLockSlim Lock { get; }
            public T Item { get; set; }

            public void Dispose()
            {
                Lock.Dispose();
            }
        }

        private static readonly CustomerSettings m_emptyCustomerSettings = new CustomerSettings(new PROPERTY_BAG[0]);

        private readonly ConcurrentDictionary<decimal, Protected<CustomerSettings>> m_customerSettings =
            new ConcurrentDictionary<decimal, Protected<CustomerSettings>>();

        private void PreloadCustomerSettings(IDataContext dc)
        {
            m_log.Debug("loading customer settings");
            var customerSettings =
                // ReSharper disable once PossibleInvalidOperationException
                GetCustomerSettingsRecords(dc).ToList().ToLookup(x => x.CUSTOMER_ID.Value);
            foreach (var pair in customerSettings.ToDictionary(x => x.Key, x => new CustomerSettings(x.ToList())))
                m_customerSettings.TryAdd(pair.Key, new Protected<CustomerSettings>(pair.Value));
            m_log.DebugFormat(
                "customer settings loaded for: {0}",
                string.Join(", ", m_customerSettings.Keys.Select(x => x.ToString("N0"))));
        }

        private IQueryable<PROPERTY_BAG> GetCustomerSettingsRecords(IDataContext dc)
        {
            return from p in dc.Db.PROPERTY_BAG
                where p.PROPERTY_FOLDER == ChatSettingsPropertyFolder
                      && p.CUSTOMER_ID != null
                select p;
        }

        public CustomerSettings GetCustomerSettings(uint customerId)
        {
            var item = m_customerSettings.GetOrAdd(customerId, _ => new Protected<CustomerSettings>(m_emptyCustomerSettings));
            return item.Lock.Read(() => item.Item);
        }

        public WritableCustomerSettings GetWritableCustomerSettings(uint customerId)
        {
            return new WritableCustomerSettings(GetCustomerSettings(customerId));
        }

        public void SaveCustomerSettings(IDataContext dc, uint customerId, WritableCustomerSettings settings)
        {
            var item = m_customerSettings.GetOrAdd(customerId, _ => new Protected<CustomerSettings>(m_emptyCustomerSettings));
            item.Lock.Write(
                () =>
                    {
                        SaveSettingsRecords(
                            dc,
                            settings.GetDirtyRecords(),
                            x =>
                                {
                                    x.CUSTOMER_ID = customerId;
                                    x.USER_ID = null;
                                });
                        item.Item = new CustomerSettings(settings);
                    });
        }

        #endregion

        #region Agent Settings

        private readonly ConcurrentDictionary<string, ReaderWriterLockSlim> m_agentLocks =
            new ConcurrentDictionary<string, ReaderWriterLockSlim>();

        private static readonly CacheItemPolicy m_agentSettingsCacheItemPolicy = new CacheItemPolicy
            {
                SlidingExpiration = TimeSpan.FromHours(1),
            };

        public AgentSettings GetAgentSettings(IDataContext dc, uint agentId)
        {
            var key = CreateAgentSettingsKey(agentId);
            var l = m_agentLocks.GetOrAdd(key, _ => new ReaderWriterLockSlim());
            return l.UpgradeableRead(
                () =>
                    {
                        return (AgentSettings)MemoryCache.Default.Get(key)
                               ?? l.Write(
                                   () =>
                                       {
                                           m_log.DebugFormat("loading agent settings for id={0}", agentId);
                                           var records = GetAgentSettingsRecords(dc, agentId).ToList();
                                           var agentSettings = new AgentSettings(records);
                                           m_log.DebugFormat("loaded {0} settings properties for agent id={1}", records.Count, agentId);

                                           MemoryCache.Default.Set(key, agentSettings, m_agentSettingsCacheItemPolicy);
                                           return agentSettings;
                                       });
                    });
        }

        private IQueryable<PROPERTY_BAG> GetAgentSettingsRecords(IDataContext dc, uint userId)
        {
            return from p in dc.Db.PROPERTY_BAG
                where p.PROPERTY_FOLDER == ChatSettingsPropertyFolder
                      && p.USER_ID == userId
                select p;
        }

        public void SaveAgentSettings(IDataContext dc, uint userId, WritableAgentSettings settings)
        {
            var key = CreateAgentSettingsKey(userId);
            var agentLock = m_agentLocks.GetOrAdd(key, _ => new ReaderWriterLockSlim());
            agentLock.Write(
                () =>
                    {
                        SaveSettingsRecords(
                            dc,
                            settings.GetDirtyRecords(),
                            x =>
                                {
                                    x.CUSTOMER_ID = null;
                                    x.USER_ID = userId;
                                });
                        MemoryCache.Default.Set(key, new AgentSettings(settings), m_agentSettingsCacheItemPolicy);
                    });
        }

        public WritableAgentSettings GetWritableAgentSettings(IDataContext dc, uint agentId)
        {
            return new WritableAgentSettings(GetAgentSettings(dc, agentId));
        }

        private static string CreateAgentSettingsKey(uint agentId)
        {
            return "AgentSettings|" + agentId;
        }

        #endregion

        private void SaveSettingsRecords(IDataContext dc, IEnumerable<PROPERTY_BAG> records, Action<PROPERTY_BAG> setRecordOwnership)
        {
            var lookup = records.ToLookup(x => IdentityGenerator.IsNew(x.PROPERTY_BAG_SKEY));
            if (lookup.Contains(true))
                foreach (var x in lookup[true])
                {
                    setRecordOwnership(x);
                    x.PROPERTY_FOLDER = ChatSettingsPropertyFolder;
                }

            if (lookup.Contains(true))
                foreach (var x1 in lookup[true])
                    x1.PROPERTY_BAG_SKEY = Convert.ToDecimal(dc.Db.InsertWithIdentity(x1));
            if (lookup.Contains(false))
                foreach (var x2 in lookup[false])
                    dc.Db.Update(x2);
        }

        public void Dispose()
        {
            m_serviceSettingsLock.Dispose();
            foreach (var x in m_customerSettings.Values) x.Dispose();
            foreach (var x in m_agentLocks.Values) x.Dispose();
        }
    }
}