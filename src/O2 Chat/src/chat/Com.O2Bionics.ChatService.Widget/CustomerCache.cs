using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Widget
{
    public sealed class CustomerCache : DailyCache<CustomerEntry>, ICustomerCache
    {
        private readonly CustomerEntry m_disabled = new CustomerEntry { Active = false, };
        private readonly ConcurrentHashSet<uint> m_activeCustomers = new ConcurrentHashSet<uint>();

        public override Task Load()
        {
            var tcpServiceClient = GlobalContainer.Resolve<ITcpServiceClient<IVisitorChatService>>();

            var activeCustomerIds = RepeatHelper.RunUntilSuccess(
                () => { return tcpServiceClient.Call(x => x.GetActiveCustomerIds()); },
                TaskConstants.StartAttempts,
                TaskConstants.StartSleepMs,
                Log,
                true);

            Clear();
            m_activeCustomers.Clear();
            if (null != activeCustomerIds)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < activeCustomerIds.Count; i++)
                {
                    Debug.Assert(0 < activeCustomerIds[i]);
                    m_activeCustomers.TryAdd(activeCustomerIds[i]);
                }
            }
#if DEBUG
            IsReady = true;
#endif
            Log.Debug("Load completed.");
            return Task.CompletedTask;
        }

        public bool TryGet(DateTime date, uint skey, out CustomerEntry entry)
        {
#if DEBUG
            CheckReady();
#endif
            Debug.Assert(date == date.RemoveTime());
            if (!m_activeCustomers.ContainsKey(skey))
            {
                entry = m_disabled;
                return true;
            }

            var days = date.ToDays();
            var currentDailyInfo = GetDailyInfo();
            entry = null;

            var result = days == currentDailyInfo.Days && currentDailyInfo.KeyValues.TryGetValue(skey, out entry);
            return result;
        }

        public bool IsActive(uint skey)
        {
            var result = m_activeCustomers.ContainsKey(skey);
            return result;
        }

        public bool Set(DateTime date, uint skey, CustomerEntry entry)
        {
#if DEBUG
            CheckReady();
#endif
            if (null == entry)
                throw new ArgumentNullException(nameof(entry));
            if (!entry.Active)
            {
                m_activeCustomers.TryRemove(skey);
                return true;
            }

            Debug.Assert(0 < skey);
            m_activeCustomers.TryAdd(skey);
            var result = TrySetImpl(
                date,
                currentDailyInfo => { currentDailyInfo.KeyValues[skey] = entry; });

            if (Log.IsDebugEnabled)
                Log.Debug($"{nameof(Set)} {result}: {skey}, {entry}, {date}");
            return result;
        }

        public bool SetSoft(DateTime date, uint skey, Action<CustomerEntry> action)
        {
            if (null == action)
                throw new ArgumentNullException(nameof(action));

            var result = TrySetImpl(
                date,
                currentDailyInfo =>
                    {
                        if (!currentDailyInfo.KeyValues.TryGetValue(skey, out var existingEntry))
                        {
                            existingEntry = new CustomerEntry { Active = true };
                            if (!currentDailyInfo.KeyValues.TryAdd(skey, existingEntry))
                            {
                                if (!currentDailyInfo.KeyValues.TryGetValue(skey, out existingEntry))
                                {
                                    Debug.Fail("Cannot Add and does not exist.");
                                    return;
                                }
                            }
                        }

                        Debug.Assert(null != existingEntry);
                        action.Invoke(existingEntry);
                    });
            if (Log.IsDebugEnabled)
                Log.Debug($"{nameof(SetSoft)} {result}: {skey}, {date}");

            return result;
        }

        private bool TrySetImpl(DateTime date, [NotNull] Action<DailyInfo<CustomerEntry>> action)
        {
#if DEBUG
            CheckReady();
#endif
            Debug.Assert(date == date.RemoveTime());
            var days = date.ToDays();
            var attempt = 0;
            do
            {
                var currentDailyInfo = GetDailyInfo();
                if (days < currentDailyInfo.Days)
                    continue;

                if (currentDailyInfo.Days < days)
                {
                    var dailyInfo = new DailyInfo<CustomerEntry>(days);
                    if (!CompareSet(currentDailyInfo, dailyInfo))
                        continue;
                    currentDailyInfo = dailyInfo;
                }

                action(currentDailyInfo);
                return true;
            } while (++attempt < TaskConstants.DateChangeAttempts);

            return false;
        }
    }
}