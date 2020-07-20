using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.ChatService.Settings;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests.WidgetLoadLimiter
{
    [TestFixture]
    public sealed class CustomerCacheNotifierTests
    {
        private readonly ChatServiceSettings m_settings = new TestChatServiceSettings();

        [Test]
        [MaxTime(2 * 1000)]
        public void Test()
        {
            var now = DateTime.UtcNow;
            var nowProvider = new TestNowProvider(now);

            var chatDatabaseFactory = Substitute.For<IChatDatabaseFactory>();
            var settingsStorage = Substitute.For<ISettingsStorage>();
            const string domains = "dom.aim";
            settingsStorage.GetCustomerSettings(TestConstants.CustomerId).Returns(_ => new CustomerSettings(new WritableCustomerSettings()));

            var rawChanges = new ConcurrentBag<KeyValuePair<DateTime, IList<KeyValuePair<uint, CustomerEntry>>>>();

            var visitorChatEventReceiver = Substitute.For<IVisitorChatEventReceiver>();
            visitorChatEventReceiver.WhenForAnyArgs(s => s.CustomersChanged(Arg.Any<DateTime>(), Arg.Any<IList<KeyValuePair<uint, CustomerEntry>>>()))
                .Do(
                    s =>
                        {
                            var key = s.Arg<DateTime>();
                            var list = s.Arg<IList<KeyValuePair<uint, CustomerEntry>>>();
                            rawChanges.Add(new KeyValuePair<DateTime, IList<KeyValuePair<uint, CustomerEntry>>>(key, list));
                        });

            var subscriberCollection = Substitute.For<ISubscriberCollection<IVisitorChatEventReceiver>>();
            subscriberCollection.WhenForAnyArgs(s => s.Publish(Arg.Any<Action<IVisitorChatEventReceiver>>())).Do(
                c =>
                    {
                        var action = c.Arg<Action<IVisitorChatEventReceiver>>();
                        action(visitorChatEventReceiver);
                    });
            var subscriptionManager = Substitute.For<ISubscriptionManager>();
            subscriptionManager.VisitorEventSubscribers.Returns(_ => subscriberCollection);

            var customerStorage = Substitute.For<ICustomerStorage>();
            var customer = new Customer(ObjectStatus.Active, "name1", domains, "IP");
            customerStorage.Get(Arg.Any<ChatDatabase>(), Arg.Any<uint>()).Returns(
                s =>
                    {
                        var id = s.Arg<uint>();
                        var customer1 = TestConstants.CustomerId == id ? customer : null;
                        return customer1;
                    });

            var unknownDomainStorage = Substitute.For<IWidgetLoadUnknownDomainStorage>();
            var widgetLoadStorage = Substitute.For<IWidgetLoadCounterStorage>();

            using (var chatDatabase = new ChatDatabase(m_settings.Database))
            using (var notifier = new CustomerCacheNotifier(
                nowProvider,
                chatDatabaseFactory,
                subscriptionManager,
                customerStorage,
                settingsStorage,
                unknownDomainStorage,
                widgetLoadStorage,
                1))
            {
                SetQuery(chatDatabaseFactory, chatDatabase);
                Assert.AreEqual(0, notifier.QueueSize, nameof(notifier.QueueSize));

                notifier.Notify(TestConstants.CustomerId);

                // Give time to process.
                Thread.Sleep(1);
            }

            var expectation = new List<KeyValuePair<DateTime, List<KeyValuePair<uint, CustomerEntry>>>>
                {
                    new KeyValuePair<DateTime, List<KeyValuePair<uint, CustomerEntry>>>(
                        now.RemoveTime(),
                        new List<KeyValuePair<uint, CustomerEntry>>
                            {
                                new KeyValuePair<uint, CustomerEntry>(
                                    TestConstants.CustomerId,
                                    new CustomerEntry
                                        {
                                            Active = true,
                                            Domains = new[] { domains },
                                        })
                            })
                };
            var changes = rawChanges.ToArray();
            changes.Should().BeEquivalentTo(expectation, nameof(changes));
        }

        private static void SetQuery(IChatDatabaseFactory chatDatabaseFactory, ChatDatabase chatDatabase)
        {
            chatDatabaseFactory.Query(Arg.Any<Func<ChatDatabase, Customer>>())
                .Returns(
                    s =>
                        {
                            var f = s.Arg<Func<ChatDatabase, Customer>>();
                            var customer = f(chatDatabase);
                            return customer;
                        });
        }
    }
}