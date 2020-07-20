using System;
using System.Linq;
using System.Runtime.Caching;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.DataModel;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using LinqToDB;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    [Explicit]
    public class SessionLoadPerformanceTests
    {
        private const int SessionsCount = 10000;
        private const int EventsCount = 50;

        public static readonly string ConnectionString =
            new JsonSettingsReader().ReadFromFile<TestSettings>().ChatServiceDatabase;

        private readonly ChatDatabaseFactory m_dbFactory = new ChatDatabaseFactory(ConnectionString, true);


        [OneTimeSetUp]
        public void SetUp()
        {
            new DatabaseManager(ConnectionString, false).RecreateSchema();
            new DatabaseManager(ConnectionString, false).ReloadData();
        }

        [Test, Explicit]
        // 1:57 - 100, connection per query
        // 0:05 - 100, single connection
        // 0:33 - 1000, single connection
        // 5:39s - 10000, single connection
        public void TestCreateChatSessions()
        {
            var now = DateTime.UtcNow;
            var customerId = 1u;
            m_dbFactory.Query(
                db =>
                    {
                        var departmentId = db.DEPARTMENTs
                            .Where(x => x.CUSTOMER_ID == customerId && x.NAME == "Sales")
                            .Select(x => x.ID)
                            .First();

                        var lastVisitorId = db.VISITORs.Any() ? db.VISITORs.Max(x => x.VISITOR_ID) + 1 : 1;

                        var visitor = new VISITOR
                            {
                                VISITOR_ID = lastVisitorId,
                                CUSTOMER_ID = customerId,
                                ADD_TIMESTAMP = now,
                                UPDATE_TIMESTAMP = now,
                                NAME = "visitor_test1",
                                EMAIL = "asd@asd.asd",
                            };
                        db.Insert(visitor);

                        var sessionsCount = SessionsCount;
                        var eventsCount = EventsCount;

                        var lastSessionId = db.CHAT_SESSION.Any()
                            ? (long)db.CHAT_SESSION.Max(x => x.CHAT_SESSION_ID)
                            : 0;
                        var lastEventId = db.CHAT_EVENT.Any()
                            ? (long)db.CHAT_EVENT.Max(x => x.CHAT_EVENT_ID)
                            : 0;
                        for (var i = 0; i < sessionsCount; i++)
                        {
                            var cs = new CHAT_SESSION
                                {
                                    CHAT_SESSION_ID = lastSessionId++,
                                    CUSTOMER_ID = customerId,
                                    CHAT_SESSION_STATUS_ID = (sbyte)ChatSessionStatus.Queued,
                                    ADD_TIMESTAMP = now,
                                    VISITOR_ID = visitor.VISITOR_ID,
                                    IS_OFFLINE = 0,
                                };
                            db.Insert(cs);

                            var evtStart = new CHAT_EVENT
                                {
                                    CHAT_SESSION_ID = cs.CHAT_SESSION_ID,
                                    CHAT_EVENT_ID = lastEventId++,
                                    CHAT_EVENT_TYPE_ID = (sbyte)ChatEventType.VisitorCreatesSessionToDept,
                                    TIMESTAMP = now,
                                    TEXT = "Hello here!",
                                    TARGET_DEPARTMENT_ID = departmentId,
                                    IS_OFFLINE_SESSION = 0,
                                };
                            db.Insert(evtStart);

                            for (var j = 0; j < eventsCount - 1; j++)
                            {
                                var evt = new CHAT_EVENT
                                    {
                                        CHAT_SESSION_ID = cs.CHAT_SESSION_ID,
                                        CHAT_EVENT_ID = lastEventId++,
                                        CHAT_EVENT_TYPE_ID = (sbyte)ChatEventType.VisitorSendsMessage,
                                        TIMESTAMP = now,
                                        TEXT = "Some message text " + j,
                                    };
                                db.Insert(evt);
                            }

                            if (i % 10 == 0) Console.WriteLine(i);
                        }
                    });
        }

        [Test, Explicit]
        // load 10000*50 - 0:15s
        public void TestLoadChatSessions()
        {
            var settings = new TestChatServiceSettings();
            var nowProvider = new TestNowProvider(DateTime.UtcNow);

            var agentStorage = new UserStorage(nowProvider);
            var settingsStorage = new SettingsStorage();
            var customerStorage = new CustomerStorage(nowProvider);
            using (var cache = new MemoryCache("test"))
            {
                var featureServiceClient = new FeatureServiceClient(settings.FeatureServiceClient, cache, nowProvider);
                var userManager = new UserManager(nowProvider, settingsStorage, agentStorage, customerStorage, featureServiceClient);
                var visitorStorage = new VisitorStorage(nowProvider, m_dbFactory, settings);
                var departmentStorage = new DepartmentStorage(nowProvider);
                var objectResolver = new ObjectResolver(m_dbFactory, agentStorage, userManager, visitorStorage, departmentStorage);
                var sessionStorage = new ChatSessionStorage(objectResolver, nowProvider);

                m_dbFactory.Query(
                    db =>
                        {
                            visitorStorage.Load(db);
                            sessionStorage.Load(db);
                        });

                Console.WriteLine("done " + sessionStorage.Count());
            }
        }

        [Test, Explicit]
        public void TestRemoveTestChatSessions()
        {
            var visitorId = m_dbFactory.Query(
                db => db.VISITORs
                    .Where(x => x.NAME == "visitor_test1")
                    .Select(x => x.VISITOR_ID)
                    .First());
            m_dbFactory.Query(db => db.CHAT_EVENT.Where(x => x.CHATEVENTSESSION.VISITOR_ID == visitorId).Delete());
            m_dbFactory.Query(db => db.CHAT_SESSION.Where(x => x.VISITOR_ID == visitorId).Delete());
            m_dbFactory.Query(db => db.VISITORs.Where(x => x.VISITOR_ID == visitorId).Delete());
        }
    }
}