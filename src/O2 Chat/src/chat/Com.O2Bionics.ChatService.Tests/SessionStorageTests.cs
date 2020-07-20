using System;
using System.Diagnostics;
using System.Runtime.Caching;
using Com.O2Bionics.ChatService.Impl;
using Com.O2Bionics.ChatService.Impl.Storage;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;
using log4net;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    public class SessionStorageTests
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(SessionStorageTests));

        public static readonly string ConnectionString =
            new JsonSettingsReader().ReadFromFile<TestSettings>().ChatServiceDatabase;

        private readonly ChatDatabaseFactory m_dbFactory = new ChatDatabaseFactory(ConnectionString, true);
        
        [Test]
        [Explicit]
        public void TestChatSessionCopyPerformance()
        {
            var settings = new TestChatServiceSettings();
            m_log.Debug(settings.Database);
            var nowProvider = new DefaultNowProvider();

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


                m_log.Debug(sessionStorage);

                var s = sessionStorage.Get(1, 2);
                s.Should().NotBeNull();

                ChatSession s1;
                for (var i = 0; i < 1000; i++)
//                s1 = s.Copy();
                    s1 = new ChatSession(s);

                const int n = 10000000;
                var sw = Stopwatch.StartNew();
                for (var i = 0; i < n; i++)
//                s1 = s.Copy();
                    s1 = new ChatSession(s);
                var rt = sw.Elapsed;

                Console.WriteLine("{0} {1} {2}", n, rt, rt.TotalMilliseconds / n);
            }
        }
    }
}