using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.ChatService.Impl.AuditTrail;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService.Impl.Storage
{
    /// <summary>
    /// An instance is thread-safe.
    /// </summary>
    [UsedImplicitly]
    public sealed class WidgetLoadCounterStorage : DailyCacheStorage<WidgetLoadCounter>, IWidgetLoadCounterStorage
    {
        private readonly IFeatureServiceClient m_featureService;
        private readonly IChatDatabaseFactory m_chatDatabaseFactory;
        private readonly WidgetLoadLimiterSettings m_widgetLoadLimiterSettings;
        private readonly ICustomerWidgetLoadStorage m_storage;
        private readonly LockFreeDateTime m_lastSaveTime = new LockFreeDateTime();

        public WidgetLoadCounterStorage(
            [NotNull] INowProvider nowProvider,
            [NotNull] IAuditTrailClient auditTrailClient,
            [NotNull] IFeatureServiceClient featureService,
            [NotNull] IChatDatabaseFactory chatDatabaseFactory,
            [NotNull] WidgetLoadLimiterSettings widgetLoadLimiterSettings,
            [NotNull] ICustomerWidgetLoadStorage storage) : base(nowProvider, auditTrailClient)
        {
            m_featureService = featureService ?? throw new ArgumentNullException(nameof(featureService));
            m_chatDatabaseFactory = chatDatabaseFactory ?? throw new ArgumentNullException(nameof(chatDatabaseFactory));
            m_widgetLoadLimiterSettings = widgetLoadLimiterSettings ?? throw new ArgumentNullException(nameof(widgetLoadLimiterSettings));
            m_storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override Task LoadSetValues(DailyInfo<WidgetLoadCounter> dailyInfo, DateTime date)
        {
            List<WidgetViewStatisticsEntry> source;
            using (var context = m_chatDatabaseFactory.CreateContext())
                source = m_storage.Get(context.Db, date).ToList();
            Log.DebugFormat("Selected {0} widget loads.", source.Count);
            SetValues(source, dailyInfo.KeyValues);
            return Task.CompletedTask;
        }

        private static void SetValues(
            [CanBeNull] List<WidgetViewStatisticsEntry> source,
            [NotNull] ConcurrentDictionary<uint, WidgetLoadCounter> target)
        {
            if (null == source)
                return;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < source.Count; i++)
            {
                var p = source[i];
                target[p.CustomerId] =
                    new WidgetLoadCounter { Total = p.Count, Status = ToStatus(p.IsViewCountExceeded) };
            }
        }

        public void LoadMany(uint[] customerIds, bool throwOnException = false)
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
                var now = NowProvider.UtcNow;
                date = now.RemoveTime();
                var days = date.ToDays();
                var currentDailyInfo = GetDailyInfo();
                if (days < currentDailyInfo.Days)
                    continue;

                if (currentDailyInfo.Days < days)
                {
                    LaunchSave(now, throwOnException).WaitAndUnwrapException();
                    continue;
                }

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < customerIds.Length; i++)
                {
                    var customerId = customerIds[i];
                    WidgetViewStatisticsEntry source;
                    using (var context = m_chatDatabaseFactory.CreateContext())
                        source = m_storage.GetForCustomer(context.Db, customerId, date, date.AddDays(1)).FirstOrDefault();

                    if (null != source) //It is acceptable to loose some loads.
                        currentDailyInfo.KeyValues[customerId] =
                            new WidgetLoadCounter { Total = source.Count, Status = ToStatus(source.IsViewCountExceeded) };
                    else
                        currentDailyInfo.KeyValues.TryRemove(customerId, out var _);
                }

                return;
            } while (++attempt < TaskConstants.DateChangeAttempts);

            throw new Exception($"All {TaskConstants.DateChangeAttempts} attempts failed to load {customerIds.Length} customers, date={date}.");
        }

        public async Task<bool> Add(uint customerId, DateTime dateTime, long pageLoads)
        {
#if DEBUG
            CheckReady();
            Debug.Assert(null != CustomerCache);
#endif
            if (customerId <= 0)
                throw new ArgumentOutOfRangeException(
                    string.Format(Utils.Properties.Resources.ArgumentMustBePositive2, nameof(customerId), customerId));
            if (pageLoads <= 0)
                throw new ArgumentOutOfRangeException(
                    string.Format(Utils.Properties.Resources.ArgumentMustBePositive2, nameof(pageLoads), pageLoads));

            var limitNew = await m_featureService.GetInt32(customerId, FeatureCodes.WidgetDailyViewLimit);
            var isUnlimited = 0 == limitNew;

            var date = dateTime.RemoveTime();
            var days = date.ToDays();
            var attempt = 0;
            do
            {
                var currentDailyInfo = GetDailyInfo();
                if (currentDailyInfo.Days < days)
                {
                    //The next day has probably started - save the old data.
                    await LaunchSave(dateTime);
                    continue;
                }

                // It is OK if a few events from the last seconds of the previous day
                // may go to the current day.
                if (!currentDailyInfo.KeyValues.TryGetValue(customerId, out var instance))
                {
                    instance = new WidgetLoadCounter();
                    if (!currentDailyInfo.KeyValues.TryAdd(customerId, instance))
                        instance = currentDailyInfo.KeyValues[customerId];
                }

                Debug.Assert(null != instance);
                instance.Limit = limitNew;
                var lastSaveTime = m_lastSaveTime.Value;
                bool shouldFlush = false, result = false;

                if (WidgetLoadStatus.None == instance.Status)
                {
                    if (isUnlimited || instance.Total <= limitNew)
                    {
                        var total = Interlocked.Add(ref instance.Total, pageLoads);
                        var increment = Interlocked.Add(ref instance.Increment, pageLoads);
                        if (!isUnlimited && limitNew < total)
                            shouldFlush = instance.AboutExceeded();

                        if (m_widgetLoadLimiterSettings.CountersDbUpdateDelta <= increment)
                            shouldFlush = true;
                    }
                    else if (instance.AboutExceeded())
                        shouldFlush = true;

                    result = isUnlimited || WidgetLoadStatus.None == instance.Status;
                }

                if (shouldFlush)
                {
                    var now = NowProvider.UtcNow;
                    var t = lastSaveTime.AddSeconds(m_widgetLoadLimiterSettings.CountersDbUpdateMinimumIntervalSeconds);
                    if (t <= now)
                    {
                        if (m_lastSaveTime.Set(lastSaveTime, now))
                            await LaunchSave(now);
                    }
                }

                return result;
            } while (++attempt < TaskConstants.DateChangeAttempts);

            throw new Exception($"Failed to add the widget loads, customer={customerId}, dateTime={dateTime}, pageLoads={pageLoads}.");
        }

        public bool HasEvent(DateTime date, uint customerId)
        {
#if DEBUG
            CheckReady();
#endif
            Debug.Assert(date == date.RemoveTime());
            var days = date.ToDays();
            var currentDailyInfo = GetDailyInfo();
            var result = days == currentDailyInfo.Days
                         && currentDailyInfo.KeyValues.TryGetValue(customerId, out var instance)
                         && WidgetLoadStatus.None != instance.Status;
            return result;
        }

        private async Task<int> LogEvent(DateTime date, uint customerId, [NotNull] WidgetLoadCounter counter, bool throwOnException)
        {
            Debug.Assert(date == date.RemoveTime());
            try
            {
                var auditEvent = new AuditEvent<WidgetDailyViewCountExceededEvent>
                    {
                        Status = OperationStatus.AccessDeniedKey,
                        Operation = OperationKind.WidgetDailyOverloadKey,
                        CustomerId = customerId.ToString(),
                        NewValue = new WidgetDailyViewCountExceededEvent
                            {
                                Total = counter.Total,
                                Limit = counter.Limit,
                                Date = date
                            },
                    };
                auditEvent.SetContextCustomValues();
                auditEvent.SetAnalyzedFields();
                await AuditTrailClient.Save(auditEvent);
                return 0;
            }
            catch (Exception e)
            {
                if (throwOnException)
                    throw;
                var message = $"Cannot add customer {customerId} overload {counter} audit message.";
                Log.Error(message, e);
            }

            return 1;
        }

        public void Dispose()
        {
            if (Log.IsDebugEnabled)
                Log.Debug("Start disposing");
            try
            {
                Save();
            }
            catch (Exception e)
            {
                Log.Error("Error disposing", e);
            }
        }

        public int Save(bool throwOnException = false)
        {
#if DEBUG
            CheckReady();
#endif
            try
            {
                var now = NowProvider.UtcNow;
                var result = LaunchSave(now, throwOnException).WaitAndUnwrapException();
                return result;
            }
            catch (Exception e)
            {
                if (throwOnException)
                    throw;
                Log.Error("Save error.", e);
            }

            return 1;
        }

        [NotNull]
        private DailyInfo<WidgetLoadCounter> GetDailyInfoToSave(DateTime dateTime)
        {
            var days = dateTime.ToDays();
            var attempt = 0;
            do
            {
                var currentDailyInfo = GetDailyInfo();
                if (currentDailyInfo.Days < days)
                {
                    var dailyInfo = new DailyInfo<WidgetLoadCounter>(days);
                    if (!CompareSet(currentDailyInfo, dailyInfo))
                        continue;
                }

                return currentDailyInfo;
            } while (++attempt < TaskConstants.DateChangeAttempts);

            throw new Exception($"All {TaskConstants.DateChangeAttempts} attempts failed to get DailyInfo at {dateTime}.");
        }

        private async Task<int> LaunchSave(DateTime now, bool throwOnException = false)
        {
            //Prevent unnecessary flushing.
            m_lastSaveTime.Value = NowProvider.UtcNow;

            var dailyInfo = GetDailyInfoToSave(now);
            var date = dailyInfo.Days.FromDays();
            var customerAndCounts = GrabChanges(dailyInfo);
            if (Log.IsDebugEnabled)
                Log.Debug($"{customerAndCounts?.Count ?? 0} customer widget loads to save for {date}.");
            if (null == customerAndCounts || 0 == customerAndCounts.Count)
                return 0;

            var saveErrors = SaveImpl(date, customerAndCounts, throwOnException);
            var lastSaveTime = NowProvider.UtcNow;
            m_lastSaveTime.Value = lastSaveTime;
            if (Log.IsDebugEnabled)
                Log.Debug($"{customerAndCounts.Count} customer page loads saved, elapsed {(lastSaveTime - now).TotalMilliseconds} ms.");

            UpdateTotal(date, customerAndCounts);
            var auditErrors = await AuditOverloads(now.RemoveTime(), customerAndCounts, throwOnException);
            return saveErrors + auditErrors;
        }

        [CanBeNull]
        private static List<KeyValuePair<uint, WidgetLoadCounter>> GrabChanges([NotNull] DailyInfo<WidgetLoadCounter> dailyInfo)
        {
            var array = dailyInfo.KeyValues.ToArray();
            if (0 == array.Length)
                return null;

            var result = new List<KeyValuePair<uint, WidgetLoadCounter>>();
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < array.Length; i++)
            {
                var p = array[i];
                var instance = p.Value;
                var increment = GetReset(ref instance.Increment);
                var status = instance.Status;

                var isAboutOverload = WidgetLoadStatus.AboutExceeded == status;
                if (isAboutOverload || 0 < increment)
                {
                    result.Add(
                        new KeyValuePair<uint, WidgetLoadCounter>(
                            p.Key,
                            new WidgetLoadCounter
                                {
                                    Increment = increment,
                                    Status = status,
                                    Limit = instance.Limit
                                }));
                    if (isAboutOverload)
                        instance.Status = WidgetLoadStatus.Exceeded;
                }
            }

            return result;
        }

        private static long GetReset(ref long value)
        {
            var count = 0;
            for (;;)
            {
                var result = Interlocked.Read(ref value);
                if (0 == result)
                    return 0;

                var compare = Interlocked.CompareExchange(ref value, default(long), result);
                if (compare == result)
                    return result;

                const int attempts = 256;
                if (attempts <= ++count)
                {
                    count = 0;
                    Thread.Sleep(0);
                }
            }
        }

        private int SaveImpl(
            DateTime date,
            [NotNull] List<KeyValuePair<uint, WidgetLoadCounter>> customerCounts,
            bool throwOnException)
        {
            var errorCount = 0;
            var count = customerCounts.Count;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < count; i++)
            {
                var p = customerCounts[i];
                var customerId = p.Key;

                //Some data can be lost.
                long currentValue = 0;
                var isOverload = false;
                try
                {
                    Action action = () =>
                        {
                            long value;
                            bool isTemp;
                            //TODO: p3. task-125. Can be done faster by using the array table parameters or bulk insert.
                            using (var dataContext = m_chatDatabaseFactory.CreateContext())
                            {
                                var upd = m_storage.Update(
                                    dataContext.Db,
                                    customerId,
                                    date,
                                    p.Value.Increment,
                                    WidgetLoadStatus.None != p.Value.Status);
                                value = upd.Key;
                                isTemp = upd.Value;
                                dataContext.Commit();
                            }

                            currentValue = value;
                            isOverload = isTemp;
                        };
                    const int attempts = 10, sleepMs = 20;
                    action.RunUntilSuccess(attempts, sleepMs, Log, throwOnException);
                }
                catch (Exception e)
                {
                    if (throwOnException)
                        throw;
                    Log.Error($"Error saving the page loads, customerId={customerId}, count={p.Value.Increment}.", e);
                    ++errorCount;
                }

                customerCounts[i] = new KeyValuePair<uint, WidgetLoadCounter>(
                    customerId,
                    new WidgetLoadCounter
                        {
                            Total = currentValue,
                            Status = ToStatus(isOverload),
                            Increment = p.Value.Increment,
                            Limit = p.Value.Limit
                        });
            }

            return errorCount;
        }

        private void UpdateTotal(DateTime date, [NotNull] List<KeyValuePair<uint, WidgetLoadCounter>> customerAndCurrentCounts)
        {
            var days = date.ToDays();
            var currentDailyInfo = GetDailyInfo();
            if (days != currentDailyInfo.Days)
                return;

            var count = customerAndCurrentCounts.Count;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < count; i++)
            {
                var p = customerAndCurrentCounts[i];
                var customerId = p.Key;
                if (currentDailyInfo.KeyValues.TryGetValue(customerId, out var instance))
                    instance.Total = p.Value.Total + instance.Increment;
#if DEBUG
                else
                {
                    throw new Exception($"DEBUG. There must be a counter for customerId={customerId}, date={date}.");
                }
#endif
            }
        }

        private async Task<int> AuditOverloads(
            DateTime date,
            [NotNull] List<KeyValuePair<uint, WidgetLoadCounter>> customerAndCurrentCounts,
            bool throwOnException)
        {
            var errorCount = 0;
            var count = customerAndCurrentCounts.Count;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < count; i++)
            {
                var p = customerAndCurrentCounts[i];
                if (WidgetLoadStatus.None == p.Value.Status)
                    continue;

                var customerId = p.Key;
                errorCount += await LogEvent(date, customerId, p.Value, throwOnException);
                CustomerCache?.Notify(customerId);
            }

            return errorCount;
        }

        private static WidgetLoadStatus ToStatus(bool isOverload)
        {
            return isOverload ? WidgetLoadStatus.Exceeded : WidgetLoadStatus.None;
        }
    }
}