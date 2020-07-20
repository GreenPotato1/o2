using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.ChatService.Tests.Mocks;
using Com.O2Bionics.MailerService.Client;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using LinqToDB;
using log4net;
using NSubstitute;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    public class VisitorChatServiceTests
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(VisitorChatServiceTests));

        private readonly ChatServiceSettings m_settings = new TestChatServiceSettings();

        [SetUp]
        public void SetUp()
        {
            new DatabaseManager(m_settings.Database, false).RecreateSchema();
            new DatabaseManager(m_settings.Database, false).ReloadData();
        }

        [Test]
        public void TestChatWindowLoad()
        {
            var utcNow = DateTime.UtcNow;
            var databaseFactory = new ChatDatabaseFactory(m_settings.Database);

            CUSTOMER customer = null;
            databaseFactory.Query(
                db =>
                    {
                        customer = new CUSTOMER
                            {
                                ID = 3,
                                CREATE_TIMESTAMP = utcNow,
                                UPDATE_TIMESTAMP = utcNow,
                                STATUS_ID = ObjectStatus.Active.ToDb(),
                                NAME = "test",
                                DOMAINS = "test.com",
                                CREATE_IP = "127.0.0.1",
                            };
                        db.Insert(customer);
                    });
            customer.Should().NotBeNull();
            customer.ID.Should().BeGreaterThan(0);

            var nowProvider = new TestNowProvider(utcNow);
            var settingsStorage = new SettingsStorage();

            var featureServiceClient = new FeatureServiceClientMock(
                (customerId, codes) =>
                    {
                        customerId.Should().Be(customer.ID);
                        m_log.DebugFormat("fsc call: {0}, ({1})", customerId, codes.JoinAsString());
                        return new Dictionary<string, string>
                            {
                                { "", "" },
                            };
                    }
            );
            var auditTrailClient = Substitute.For<IAuditTrailClient>();
            auditTrailClient.SelectFacets(Arg.Any<Filter>())
                .ReturnsForAnyArgs(
                    ci =>
                        {
                            m_log.DebugFormat("atc call: {0}", ((Filter)ci.Args()[0]).JsonStringify2());
                            return Task.FromResult<FacetResponse>(null);
                        });

            var subscriptionManager = new SubscriptionManager(settingsStorage);

            var visitorStorage = new VisitorStorage(nowProvider, databaseFactory, m_settings);
            databaseFactory.Query(db => visitorStorage.Load(db));
            var userStorage = new UserStorage(nowProvider);
            var departmentStorage = new DepartmentStorage(nowProvider);
            var customerStorage = new CustomerStorage(nowProvider);
            var agentSessionStorage = new AgentSessionStorage(nowProvider);
            databaseFactory.Query(db => agentSessionStorage.Load(db));

            var userManager = new UserManager(
                nowProvider,
                settingsStorage,
                userStorage,
                customerStorage,
                featureServiceClient);

            var objectResolver = new ObjectResolver(
                databaseFactory,
                userStorage,
                userManager,
                visitorStorage,
                departmentStorage);

            var chatSessionStorage = new ChatSessionStorage(objectResolver, nowProvider);
            databaseFactory.Query(db => chatSessionStorage.Load(db));

            var chatSessionManager = new ChatSessionManager(
                chatSessionStorage,
                visitorStorage,
                objectResolver,
                subscriptionManager);

            var agentManager = new AgentManager(
                subscriptionManager,
                chatSessionManager,
                settingsStorage,
                userStorage,
                departmentStorage,
                chatSessionStorage,
                agentSessionStorage,
                nowProvider,
                databaseFactory);

            var chatWidgetAppearanceManager = new ChatWidgetAppearanceManager(
                settingsStorage,
                databaseFactory,
                featureServiceClient);

            var customerWidgetLoadStorage = new CustomerWidgetLoadStorage();
            var unknownDomainLoader = new UnknownDomainLoader();
            var widgetLoadCounterStorage = new WidgetLoadCounterStorage(
                nowProvider,
                auditTrailClient,
                featureServiceClient,
                databaseFactory,
                m_settings.WidgetLoadLimiterSettings,
                customerWidgetLoadStorage);
            widgetLoadCounterStorage.Load().WaitAndUnwrapException();
            var widgetLoadUnknownDomainStorage = new WidgetLoadUnknownDomainStorage(
                nowProvider,
                auditTrailClient,
                unknownDomainLoader);
            widgetLoadUnknownDomainStorage.Load().WaitAndUnwrapException();

            using (var customerCacheNotifier = new CustomerCacheNotifier(
                nowProvider,
                databaseFactory,
                subscriptionManager,
                customerStorage,
                settingsStorage,
                widgetLoadUnknownDomainStorage,
                widgetLoadCounterStorage,
                1))
            {
                widgetLoadCounterStorage.SetNotifier(customerCacheNotifier);
                widgetLoadUnknownDomainStorage.SetNotifier(customerCacheNotifier);

                var visitorChatService = new VisitorChatService(
                    nowProvider,
                    databaseFactory,
                    visitorStorage,
                    settingsStorage,
                    agentManager,
                    chatSessionManager,
                    subscriptionManager,
                    chatWidgetAppearanceManager,
                    m_settings,
                    new MailerServiceClient(m_settings.MailerServiceClient),
                    userManager,
                    widgetLoadUnknownDomainStorage,
                    widgetLoadCounterStorage,
                    customerCacheNotifier,
                    customerStorage);

                var result = visitorChatService.ChatWindowLoad(customer.ID, 0, "test.com", false);
                result.Should().NotBeNull();
            }
        }
    }
}