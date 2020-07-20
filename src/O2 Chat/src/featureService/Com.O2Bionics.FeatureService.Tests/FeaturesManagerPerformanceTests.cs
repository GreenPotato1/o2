using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Com.O2Bionics.FeatureService.Impl;
using Com.O2Bionics.FeatureService.Impl.DataModel;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.FeatureService.Tests
{
    [TestFixture]
    [Explicit]
    public class FeaturesManagerPerformanceTests
    {
        private const int IterationsNumber = 10000;
        private const int CustomerNumber = 1000;

        protected const string Feature1Code = "testFeature1";
        protected const string Feature2Code = "testFeature2";

        private static readonly FeatureServiceTestSettings m_testSettings =
            new FeatureServiceTestSettings { LogSqlQuery = false, LogProcessing = false };

        private readonly DatabaseHelper m_dbh =
            new DatabaseHelper(m_testSettings);

        protected IFeaturesManager CreateFeaturesManager()
        {
            return new FeaturesManager(m_dbh.Settings, m_dbh.DbFactory, null);
        }

        [SetUp]
        public void SetUp()
        {
            m_dbh.DbFactory.LogEnabled = false;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            PrepareDatabase();
        }

        public void CleanupDatabase()
        {
            m_dbh.CleanupDatabase();
        }

        public void PrepareDatabase()
        {
            m_dbh.CleanupDatabase();

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        var feature1Id = dboh.AddFeature(Feature1Code, FeatureValueAggregationMethod.CAS);
                        var feature2Id = dboh.AddFeature(Feature2Code);

                        var mainPlanId = dboh.AddService("mainPlan", 0);
                        var addon1Id = dboh.AddService("addon1", 1);
                        var addon2Id = dboh.AddService("addon2", 1);
                        var addon3Id = dboh.AddService("addon3", 1);

                        dboh.AddServiceFeatureValue(mainPlanId, feature1Id, "1");
                        dboh.AddServiceFeatureValue(addon1Id, feature1Id, "2");
                        dboh.AddServiceFeatureValue(addon2Id, feature1Id, "3");
                        dboh.AddServiceFeatureValue(addon3Id, feature1Id, "4");

                        dboh.AddServiceFeatureValue(addon2Id, feature2Id, "3");

                        for (var i = 0; i < CustomerNumber; i++)
                        {
                            var userId = dboh.AddCustomer("customer_" + i);
                            dboh.AddServiceSubscription(mainPlanId, userId);
                            dboh.AddServiceSubscription(addon1Id, userId);
                            dboh.AddServiceSubscription(addon2Id, userId);
                            dboh.AddServiceSubscription(addon3Id, userId);
                        }
                    });
        }

        [Test]
        public void TestCasAggregationFeatureValueDirectCall()
        {
            var testCustomerId = GetTestCustomerId();

            var fm = CreateFeaturesManager();

            // warmup
            for (var i = 0; i < 100; i++)
                fm.GetFeatureValue(DatabaseHelper.TestProductCode, testCustomerId, new HashSet<string> { Feature1Code });

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < IterationsNumber; i++)
                fm.GetFeatureValue(DatabaseHelper.TestProductCode, testCustomerId, new HashSet<string> { Feature1Code });

            sw.Stop();
            Console.WriteLine("n: {0}, time: {1}, one: {2}ms.", IterationsNumber, sw.Elapsed, (double)sw.ElapsedMilliseconds / IterationsNumber);
        }

        [Test]
        public void TestCasAggregationFeatureValueDirectCall2()
        {
            var testCustomerId = GetTestCustomerId();

            var fm = CreateFeaturesManager();

            // warmup
            for (var i = 0; i < 100; i++)
                fm.GetFeatureValue(DatabaseHelper.TestProductCode, testCustomerId, new HashSet<string> { Feature1Code, Feature2Code });

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < IterationsNumber; i++)
                fm.GetFeatureValue(DatabaseHelper.TestProductCode, testCustomerId, new HashSet<string> { Feature1Code, Feature2Code });

            sw.Stop();
            Console.WriteLine("n: {0}, time: {1}, one: {2}ms.", IterationsNumber, sw.Elapsed, (double)sw.ElapsedMilliseconds / IterationsNumber);
        }

        [Test]
        public async Task TestCasAggregationFeatureValueWebServiceCall([Values(true, false)] bool ignoreCache)
        {
            using (var server = new FeatureServiceTestServerHelper())
            {
                var testCustomerId = GetTestCustomerId();

                using (var client = server.CreateClient(null, ignoreCache))
                {
                    // warmup
                    for (var i = 0; i < 100; i++)
                        await client.GetValue((uint)testCustomerId, new List<string> { Feature1Code });

                    var sw = Stopwatch.StartNew();

                    for (var i = 0; i < IterationsNumber; i++)
                        await client.GetValue((uint)testCustomerId, new List<string> { Feature1Code });

                    sw.Stop();
                    Console.WriteLine(
                        "n: {0}, time: {1}, one: {2}ms.",
                        IterationsNumber,
                        sw.Elapsed,
                        (double)sw.ElapsedMilliseconds / IterationsNumber);
                }
            }
        }

        private int GetTestCustomerId()
        {
            return m_dbh.Query(
                db =>
                    {
                        using (var cmd = new OracleCommand("select USERID from CUSTOMER order by USERID"))
                        {
                            var ids = new List<int>();
                            using (var reader = db.ExecuteReader(cmd))
                            {
                                while (reader.Read()) ids.Add(reader.GetInt32(0));
                            }

                            return ids[ids.Count / 2];
                        }
                    });
        }
    }
}