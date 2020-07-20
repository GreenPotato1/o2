using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Properties;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    public sealed class WidgetLoadUnknownDomainStorage : DailyCacheStorage<UnknownDomainCounter>, IWidgetLoadUnknownDomainStorage
    {
        private readonly IUnknownDomainLoader m_unknownDomainLoader;

        private int m_maximumUnknownDomains = DomainUtilities.DefaultMaximumUnknownDomains;

        internal int MaximumUnknownDomains
        {
            get => m_maximumUnknownDomains;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(string.Format(Resources.ArgumentMustBePositive2, nameof(value), value));
                m_maximumUnknownDomains = value;
            }
        }

        public WidgetLoadUnknownDomainStorage(
            [NotNull] INowProvider nowProvider,
            [NotNull] IAuditTrailClient auditTrailClient,
            [NotNull] IUnknownDomainLoader unknownDomainLoader) : base(nowProvider, auditTrailClient)
        {
            m_unknownDomainLoader = unknownDomainLoader ?? throw new ArgumentNullException(nameof(unknownDomainLoader));
        }

        protected override async Task LoadSetValues(DailyInfo<UnknownDomainCounter> dailyInfo, DateTime date)
        {
            var source = await m_unknownDomainLoader.Load(AuditTrailClient, date, m_maximumUnknownDomains);
            Log.DebugFormat("Selected {0} unknown domains.", source?.Count ?? 0);
            SetValues(source, dailyInfo.KeyValues);
        }

        private static void SetValues(
            [CanBeNull] Dictionary<uint, HashSet<string>> source,
            [NotNull] ConcurrentDictionary<uint, UnknownDomainCounter> target)
        {
            if (null == source)
                return;
            foreach (var p in source)
                target[p.Key] = UnknownDomainCounter.FromHashSet(p.Value);
        }

        public async Task LoadMany(uint[] customerIds)
        {
#if DEBUG
            CheckReady();
#endif
            if (null == customerIds || 0 == customerIds.Length)
                throw new ArgumentNullException(nameof(customerIds));
            DateTime date;
            var attempt = 0;
            do
            {
                date = NowProvider.UtcNow.RemoveTime();
                var days = date.ToDays();
                var currentDailyInfo = GetDailyInfo();
                if (days < currentDailyInfo.Days)
                    continue;

                if (currentDailyInfo.Days < days)
                {
                    var dailyInfo = new DailyInfo<UnknownDomainCounter>(days);
                    if (!CompareSet(currentDailyInfo, dailyInfo))
                        continue;
                    currentDailyInfo = dailyInfo;
                }

                var source = await m_unknownDomainLoader.LoadMany(AuditTrailClient, date, m_maximumUnknownDomains, customerIds);
                Log.DebugFormat("Selected {0} unknown domains for {1} customerIds.", source?.Count ?? 0, customerIds.Length);

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < customerIds.Length; i++)
                {
                    var key = customerIds[i];
                    if (null != source && source.TryGetValue(key, out var hashSet))
                        currentDailyInfo.KeyValues[key] = UnknownDomainCounter.FromHashSet(hashSet);
                    else
                        currentDailyInfo.KeyValues.TryRemove(key, out var _);
                }

                return;
            } while (++attempt < TaskConstants.DateChangeAttempts);

            throw new Exception(string.Format(Properties.Resources.LoadFailedForDateError1, date));
        }

        public async Task<bool> Add(DateTime date, uint customerId, string domains, string unknownDomain, bool throwOnException = false)
        {
#if DEBUG
            CheckReady();
            Debug.Assert(null != CustomerCache);
#endif
            if (customerId <= 0)
                throw new ArgumentOutOfRangeException(
                    string.Format(Resources.ArgumentMustBePositive2, nameof(customerId), customerId));
            if (string.IsNullOrEmpty(unknownDomain))
                throw new ArgumentNullException(nameof(unknownDomain));

            date = date.RemoveTime();
            var days = date.ToDays();
            var attempt = 0;
            do
            {
                var currentDailyInfo = GetDailyInfo();

                UnknownDomainCounter instance;
                if (currentDailyInfo.Days < days)
                {
                    var dailyInfo = new DailyInfo<UnknownDomainCounter>(days);
                    dailyInfo.KeyValues[customerId] = instance = new UnknownDomainCounter();
                    if (!CompareSet(currentDailyInfo, dailyInfo))
                        continue;
                }
                else
                {
                    if (currentDailyInfo.KeyValues.TryGetValue(customerId, out instance))
                    {
                        var count = instance.GetCount();
                        if (m_maximumUnknownDomains <= count || instance.Names.ContainsKey(unknownDomain))
                            return false;
                    }
                    else
                    {
                        instance = new UnknownDomainCounter();
                        if (!currentDailyInfo.KeyValues.TryAdd(customerId, instance))
                        {
                            instance = currentDailyInfo.KeyValues[customerId];
                            Debug.Assert(null != instance);
                        }
                    }
                }

                if (!instance.Names.TryAdd(unknownDomain))
                    return false;

                var count1 = instance.IncrementCount();
                if (m_maximumUnknownDomains < count1)
                    return false;

                var isTooMany = m_maximumUnknownDomains == count1;
                var skey = customerId.ToString();
                if (isTooMany)
                {
                    var auditEvent = new AuditEvent<WidgetUnknownDomainTooManyEvent>
                        {
                            Operation = OperationKind.WidgetUnknownDomainTooManyKey,
                            NewValue = new WidgetUnknownDomainTooManyEvent
                                {
                                    Domains = domains,
                                    Limit = m_maximumUnknownDomains,
                                    Date = date
                                }
                        };
                    await LogEvent(skey, unknownDomain, auditEvent, throwOnException);
                    CustomerCache?.Notify(customerId);
                }
                else
                {
                    var auditEvent = new AuditEvent<WidgetUnknownDomain>
                        {
                            Operation = OperationKind.WidgetUnknownDomainKey,
                            NewValue = new WidgetUnknownDomain
                                {
                                    Domains = domains,
                                    Name = unknownDomain
                                }
                        };
                    await LogEvent(skey, unknownDomain, auditEvent, throwOnException);
                }

                return isTooMany;
            } while (++attempt < TaskConstants.DateChangeAttempts);

            throw new Exception(string.Format(Properties.Resources.AddAuditEventFailedError1, date, customerId, unknownDomain));
        }

        public bool HasEvent(DateTime date, uint customerId, out HashSet<string> unknownDomains)
        {
#if DEBUG
            CheckReady();
#endif
            Debug.Assert(date == date.RemoveTime());

            UnknownDomainCounter instance = null;
            var days = date.ToDays();
            var currentDailyInfo = GetDailyInfo();

            var result = days == currentDailyInfo.Days
                         && currentDailyInfo.KeyValues.TryGetValue(customerId, out instance)
                         && m_maximumUnknownDomains <= instance.GetCount();
            unknownDomains = instance?.Names.ToHashSet();

            return result;
        }

        private async Task LogEvent<T>([NotNull] string customerId, string unknownDomain, [NotNull] AuditEvent<T> auditEvent, bool throwOnException)
            where T : class
        {
            try
            {
                auditEvent.Status = OperationStatus.AccessDeniedKey;
                auditEvent.CustomerId = customerId;

                auditEvent.SetContextCustomValues();
                auditEvent.SetAnalyzedFields();
                await AuditTrailClient.Save(auditEvent);
            }
            catch (Exception e)
            {
                if (throwOnException)
                    throw;

                var message = $"Customer {customerId} has unknown domain '{unknownDomain}' audit message.";
                Log.Error(message, e);
            }
        }
    }
}