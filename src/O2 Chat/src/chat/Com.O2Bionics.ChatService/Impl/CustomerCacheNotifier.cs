using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Unity.Attributes;

namespace Com.O2Bionics.ChatService.Impl
{
    public sealed class CustomerCacheNotifier : BackgroundQueueProcessor<uint>, ICustomerCacheNotifier
    {
        private const int AddBufferSize = 256;

        private static readonly CustomerEntry m_disabledCustomerEntry = new CustomerEntry();

        private readonly INowProvider m_nowProvider;

        private readonly ISubscriptionManager m_subscriptionManager;

        private readonly ICustomerStorage m_customerStorage;

        private readonly ISettingsStorage m_settingsStorage;

        private readonly IChatDatabaseFactory m_chatDatabaseFactory;

        private readonly IWidgetLoadUnknownDomainStorage m_widgetLoadUnknownDomainStorage;

        private readonly IWidgetLoadCounterStorage m_widgetLoadCounterStorage;

        [InjectionConstructor]
        public CustomerCacheNotifier(
            [NotNull] INowProvider nowProvider,
            [NotNull] IChatDatabaseFactory chatDatabaseFactory,
            [NotNull] ISubscriptionManager subscriptionManager,
            [NotNull] ICustomerStorage customerStorage,
            [NotNull] ISettingsStorage settingsStorage,
            [NotNull] IWidgetLoadUnknownDomainStorage widgetLoadUnknownDomainStorage,
            [NotNull] IWidgetLoadCounterStorage widgetLoadCounterStorage)
            : this(
                nowProvider,
                chatDatabaseFactory,
                subscriptionManager,
                customerStorage,
                settingsStorage,
                widgetLoadUnknownDomainStorage,
                widgetLoadCounterStorage,
                AddBufferSize)
        {
        }

        public CustomerCacheNotifier(
            [NotNull] INowProvider nowProvider,
            [NotNull] IChatDatabaseFactory chatDatabaseFactory,
            [NotNull] ISubscriptionManager subscriptionManager,
            [NotNull] ICustomerStorage customerStorage,
            [NotNull] ISettingsStorage settingsStorage,
            [NotNull] IWidgetLoadUnknownDomainStorage widgetLoadUnknownDomainStorage,
            [NotNull] IWidgetLoadCounterStorage widgetLoadCounterStorage,
            int bufferSize)
            : base(bufferSize.MustBePositive(nameof(bufferSize)), new TimeSpan(0, 0, 1))
        {
            m_nowProvider = nowProvider ?? throw new ArgumentNullException(nameof(nowProvider));
            m_subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
            m_customerStorage = customerStorage ?? throw new ArgumentNullException(nameof(customerStorage));
            m_settingsStorage = settingsStorage ?? throw new ArgumentNullException(nameof(settingsStorage));
            m_chatDatabaseFactory = chatDatabaseFactory ?? throw new ArgumentNullException(nameof(chatDatabaseFactory));
            m_widgetLoadUnknownDomainStorage =
                widgetLoadUnknownDomainStorage ?? throw new ArgumentNullException(nameof(widgetLoadUnknownDomainStorage));
            m_widgetLoadCounterStorage = widgetLoadCounterStorage ?? throw new ArgumentNullException(nameof(widgetLoadCounterStorage));
        }

        public void NotifyMany(uint[] customerIds)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < customerIds.Length; i++)
                Add(customerIds[i]);
        }

        protected override void Save(List<uint> buffer)
        {
            var date = m_nowProvider.UtcNow.RemoveTime();

            var set = new HashSet<uint>();
            var list = new List<KeyValuePair<uint, CustomerEntry>>(buffer.Count);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < buffer.Count; i++)
            {
                var id = buffer[i];
                if (!set.Add(id))
                    continue;

                var customerEntry = BuildCustomerEntry(date, id);
                if (null != customerEntry)
                    list.Add(new KeyValuePair<uint, CustomerEntry>(id, customerEntry));
            }

            if (0 == list.Count)
                return;

            var tasks = m_subscriptionManager.VisitorEventSubscribers?.Publish(r => r.CustomersChanged(date, list));
            if (null != tasks && 0 < tasks.Length)
                Task.WhenAll(tasks).WaitAndUnwrapException();
        }

        [CanBeNull]
        private CustomerEntry BuildCustomerEntry(DateTime date, uint customerId)
        {
            var customer = m_chatDatabaseFactory.Query(db => m_customerStorage.Get(db, customerId));
            if (null == customer)
                return null;
            if (ObjectStatus.Active != customer.Status)
                return m_disabledCustomerEntry;

            var settings = m_settingsStorage.GetCustomerSettings(customerId);
            Debug.Assert(null != settings);

            var isOverload = m_widgetLoadCounterStorage.HasEvent(date, customerId);
            HashSet<string> unknownDomains = null;
            var isManyUnknownDomains =
                !isOverload && m_widgetLoadUnknownDomainStorage.HasEvent(date, customerId, out unknownDomains);
            var allowedDomains = DomainUtilities.GetDomains(customer.Domains);
            var result = new CustomerEntry
                {
                    Active = true,
                    ViewCounterExceeded = isOverload,
                    UnknownDomainNumberExceeded = isManyUnknownDomains,
                    Domains = allowedDomains
                };
            if (!isOverload && !isManyUnknownDomains && null != unknownDomains && 0 < unknownDomains.Count)
                result.UnknownDomains = new ConcurrentHashSet<string>(unknownDomains);

            return result;
        }
    }
}