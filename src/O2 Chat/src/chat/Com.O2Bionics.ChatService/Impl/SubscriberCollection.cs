using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Settings;
using Com.O2Bionics.Utils;
using log4net;
using Newtonsoft.Json;

namespace Com.O2Bionics.ChatService.Impl
{
    public abstract class SubscriberCollection
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(SubscriberCollection));
    }

    public class SubscriberCollection<TService> : SubscriberCollection, ISubscriberCollection<TService>, IDisposable
        where TService : IPingable
    {
        private readonly ISettingsStorage m_settingsStorage;
        private readonly Func<ServiceSettings, string> m_readFromSettings;
        private readonly Action<WritableServiceSettings, string> m_writeToSettings;

        private readonly ReaderWriterLockSlim m_lock = new ReaderWriterLockSlim();

        private readonly Dictionary<Subscriber, TcpServiceClient<TService>> m_subscribers =
            new Dictionary<Subscriber, TcpServiceClient<TService>>();

        public SubscriberCollection(
            ISettingsStorage settingsStorage,
            Func<ServiceSettings, string> readFromSettings,
            Action<WritableServiceSettings, string> writeToSettings)
        {
            m_settingsStorage = settingsStorage;
            m_readFromSettings = readFromSettings;
            m_writeToSettings = writeToSettings;
        }

        public void Load(IDataContext dc)
        {
            Log.InfoFormat("Loading subscriptions for {0}", typeof(TService).Name);

            var currentJson = m_readFromSettings(m_settingsStorage.GetServiceSettings());
            var subscriptions = ParseAndValidateSubscriptions(currentJson);
            Save(dc, subscriptions, currentJson);

            foreach (var s in subscriptions) AddToDictionary(s);

            Log.InfoFormat(
                "Loaded subscriptions for {0}: [{1}]",
                typeof(TService).Name,
                string.Join(", ", subscriptions.Select(x => x.ToString())));
        }

        private void Save(IDataContext dc, IEnumerable<Subscriber> subscribers, string currentJson = null)
        {
            var stringList = subscribers
                .Select(x => x.ToString())
                .OrderBy(x => x)
                .ToList();
            var json = JsonConvert.SerializeObject(stringList);
            if (json != currentJson)
            {
                var writableSettings = m_settingsStorage.GetWritableServiceSettings();
                m_writeToSettings(writableSettings, json);
                m_settingsStorage.SaveServiceSettings(dc, writableSettings);
            }
        }

        private static List<Subscriber> ParseAndValidateSubscriptions(string currentJson)
        {
            var subscriptionsList = Safe.Do(
                () => currentJson.AsStringList(),
                e => Log.Error($"Failed to parse subscriber list '{currentJson}'.", e));

            return subscriptionsList.Any()
                ? subscriptionsList
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Select(x => x.ToLower())
                    .Distinct()
                    .AsParallel()
                    .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                    .Select(SafeParseSubscriber)
                    .Where(x => x != null)
//                    .Select(SafePingSubscriber)
//                    .Where(x => x != null)
                    .OrderBy(x => x.ToString())
                    .ToList()
                : new List<Subscriber>();
        }

        private static Subscriber SafePingSubscriber(Subscriber x)
        {
            return Safe.Do(
                () =>
                    {
                        Log.DebugFormat("Ping {0} at {1}", typeof(TService).Name, x);
                        using (var c = new TcpServiceClient<TService>(x.Host, x.Port)) c.Call(s => s.Ping());
                        return x;
                    },
                e => Log.WarnFormat("Failed to ping subscriber {0}: {1}", x, FormatException(e)));
        }

        private static Subscriber SafeParseSubscriber(string x)
        {
            return Safe.Do(
                () => new Subscriber(x),
                e => Log.WarnFormat("Failed to parse subscriber '{0}': {1}.", x, e));
        }

        public void Add(IDataContext dc, Subscriber subscriber)
        {
            m_lock.Write(
                () =>
                    {
                        AddToDictionary(subscriber);
                        Save(dc, m_subscribers.Keys);
                    });
        }

        private void AddToDictionary(Subscriber subscriber)
        {
            if (!m_subscribers.ContainsKey(subscriber))
                m_subscribers.Add(subscriber, new TcpServiceClient<TService>(subscriber.Host, subscriber.Port));
        }

        public void Remove(IDataContext dc, Subscriber subscriber)
        {
            m_lock.Write(
                () =>
                    {
                        RemoveFromDictionary(subscriber);
                        Save(dc, m_subscribers.Keys);
                    });
        }

        private void RemoveFromDictionary(Subscriber subscriber)
        {
            TcpServiceClient<TService> client;
            if (m_subscribers.TryGetValue(subscriber, out client))
            {
                client.Dispose();
                m_subscribers.Remove(subscriber);
            }
        }

        public Task[] Publish(Action<TService> action)
        {
            Task[] result = null;

            m_lock.Read(
                () =>
                    {
                        result = new Task[m_subscribers.Count];
                        var i = 0;
                        foreach (var s in m_subscribers)
                        {
                            result[i++] = Task.Factory.StartNew(
                                () =>
                                    Safe.Do(
                                        () => s.Value.Call(action),
                                        e => Log.WarnFormat("Publishing event to {0} failed: {1}", s.Key, FormatException(e))
                                    )
                            );
                        }
                    });
            return null != result && 0 < result.Length ? result : null;
        }

        public List<TResult> Call<TResult>(Func<TService, TResult> call)
        {
            return m_lock.Read(
                () =>
                    m_subscribers
                        .Select(
                            s =>
                                Safe.Do(
                                    () => s.Value.Call(call),
                                    e => Log.WarnFormat("Calling method of {0} failed: {1}", s.Key, FormatException(e)))
                        )
                        .ToList()
            );
        }

        private static string FormatException(Exception e)
        {
            var endpointNotFoundException = e as EndpointNotFoundException;
            if (endpointNotFoundException != null)
            {
                return endpointNotFoundException.Message +
                       (endpointNotFoundException.InnerException != null
                           ? ": " + endpointNotFoundException.InnerException.Message
                           : "");
            }
            else return e.ToString();
        }

        public void Dispose()
        {
            m_lock.Write(
                () =>
                    {
                        foreach (var x in m_subscribers.Values) x.Dispose();
                    });
            m_lock.Dispose();
        }
    }
}