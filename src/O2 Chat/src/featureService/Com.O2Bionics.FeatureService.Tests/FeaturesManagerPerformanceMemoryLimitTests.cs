using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Impl.DataModel;
using Com.O2Bionics.Utils;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.FeatureService.Tests
{
    [TestFixture]
    [Explicit]
    public class FeaturesManagerPerformanceMemoryLimitTests
    {
        private const int FeatureCount = 100;
        private const int UserCount = 100;

        private readonly DatabaseHelper m_dbh =
            new DatabaseHelper(new FeatureServiceTestSettings { LogSqlQuery = false, LogProcessing = false });

        [SetUp]
        public void SetUp()
        {
            m_dbh.DbFactory.LogEnabled = false;
        }

        [Test]
        [Explicit]
        public void CleanupDatabase()
        {
            m_dbh.CleanupDatabase();
        }

        [Test]
        [Explicit]
        public void PrepareDatabase()
        {
            m_dbh.CleanupDatabase();

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        var featureIds = Enumerable.Range(0, FeatureCount).Select(i => dboh.AddFeature("feature-code-" + i)).ToList();
                        var serviceIds = featureIds.Select((x, i) => dboh.AddService("plan-" + i, 0)).ToList();

                        for (var i = 0; i < serviceIds.Count; i++)
                            dboh.AddServiceFeatureValue(serviceIds[i], featureIds[i], new string('*', 1020));

                        var userIds = Enumerable.Range(0, UserCount).Select(i => dboh.AddCustomer("user-" + i)).ToList();
                        foreach (var userId in userIds)
                        foreach (var serviceId in serviceIds)
                            dboh.AddServiceSubscription(serviceId, userId);
                    });
        }

        [Test]
        [Explicit]
        public void MemoryLimitTest()
        {
            var userIds = m_dbh.Query(
                db =>
                    {
                        using (var cmd = new OracleCommand("select userid from customer"))
                        using (var reader = db.ExecuteReader(cmd))
                        {
                            var result = new List<int>();
                            while (reader.Read())
                                result.Add(reader.GetInt32(0));
                            return result;
                        }
                    });

            var featureCodes = m_dbh.Query(
                db =>
                    {
                        using (var cmd = new OracleCommand("select feature_code from features"))
                        using (var reader = db.ExecuteReader(cmd))
                        {
                            var result = new List<string>();
                            while (reader.Read())
                                result.Add(reader.GetString(0));
                            return result;
                        }
                    });

            Console.WriteLine("users: {0}, features: {1}", userIds.Count, featureCodes.Count);

            var nowProvider = new DefaultNowProvider();
            var settings = new TestFeatureServiceClientSettings(
                DatabaseHelper.TestProductCode,
                new[] { new Uri("http://localhost:8080") });
            using (var cache = new MemoryCache("test"))
            using (var client1 = new FeatureServiceClient(settings, cache, nowProvider, null, true, null, false, false))
            {
                // warm up
                for (var i = 0; i < 10; i++)
                    client1.GetValue((uint)userIds[0], new List<string> { featureCodes[0] }).WaitAndUnwrapException();
            }

            using (var cache = new MemoryCache("test"))
            using (var client2 = new FeatureServiceClient(
                settings,
                cache,
                nowProvider,
                null,
                false,
                60 * 60 * 24,
                false,
                false))
            {
                var threads = CreateThreads(userIds, featureCodes, client2);
                var sw = Stopwatch.StartNew();
                foreach (var thread in threads) thread.Start();
                foreach (var thread in threads) thread.Join();
                sw.Stop();
                Console.WriteLine("n: {0}, time: {1}, time: {2}ms.", threads.Count, sw.Elapsed, sw.ElapsedMilliseconds);
            }
        }

        private static List<Thread> CreateThreads(List<int> userIds, List<string> featureCodes, FeatureServiceClient client)
        {
            var threads = Enumerable.Range(0, 64)
                .Select(
                    i => new Thread(
                        () =>
                            {
                                try
                                {
                                    foreach (var userId in userIds)
                                    {
                                        foreach (var featureCode in featureCodes)
                                        {
                                            client.GetValue(
                                                    (uint)userId,
                                                    new List<string> { featureCode })
                                                .WaitAndUnwrapException();
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("exception: " + e);
                                }
                            }) { IsBackground = true, Name = "thrd-" + i })
                .ToList();
            return threads;
        }

        [Test]
        [Explicit]
        public void MemoryCacheBehaviorTest()
        {
            var ttl = TimeSpan.FromDays(1);
            var config = new NameValueCollection
                {
                    { "CacheMemoryLimitMegabytes", "10" },
                    { "PhysicalMemoryLimitPercentage", "0" },
                    { "PollingInterval", "0:0:1" },
                };
            using (var cache = new MemoryCache("test cache", config))
            {
                var i = 0;
                var sw = Stopwatch.StartNew();
                while (true)
                {
                    var policy = new CacheItemPolicy
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.Add(ttl),
                        };
                    cache.Add("key-" + i, new string('*', 1024), policy);
                    i++;
                    if (i % 10000 == 0) Console.WriteLine("{0} {1}", sw.ElapsedMilliseconds, i);
                }
            }

            // ReSharper disable once FunctionNeverReturns
        }

//        [Test]
//        [Explicit]
//        public void TestCasAggregationFeatureValueWebServiceCall([Values(true, false)] bool ignoreCache)
//        {
//            var testCustomerId = GetTestCustomerId();
//
//            //            var client = new FeatureServiceClient(new[] { new Uri("http://feature-service.o2bionics.com/") }, TimeSpan.FromSeconds(5));
//
//            // warmup
//            for (var i = 0; i < 100; i++)
//                client.GetValue(DatabaseHelper.TestProductCode, testCustomerId, new List<string> { Feature1Code }, ignoreCache);
//
//            var sw = Stopwatch.StartNew();
//
//            for (var i = 0; i < IterationsNumber; i++)
//                client.GetValue(DatabaseHelper.TestProductCode, testCustomerId, new List<string> { Feature1Code }, ignoreCache);
//
//            sw.Stop();
//            Console.WriteLine("n: {0}, time: {1}, one: {2}ms.", IterationsNumber, sw.Elapsed, (double)sw.ElapsedMilliseconds / IterationsNumber);
//        }
    }
}