using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Impl;
using Com.O2Bionics.FeatureService.Impl.DataModel;
using Com.O2Bionics.Utils;
using FluentAssertions;
using NUnit.Framework;

namespace Com.O2Bionics.FeatureService.Tests
{
    [TestFixture]
    public sealed class FeatureServiceClientTests
    {
        private const string Feature1Code = "testFeature1";
        private const string Feature2Code = "testFeature2";

        private readonly DatabaseHelper m_dbh = new DatabaseHelper(new FeatureServiceTestSettings());

        [SetUp]
        public void SetUp()
        {
            m_dbh.CleanupDatabase();
        }

        [Test]
        public void TestGetValueInvalidArguments()
        {
            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);
                        dboh.AddFeature(Feature1Code);
                    });

            using (var server = new FeatureServiceTestServerHelper())
            {
                using (var c = server.CreateClient(null, true))
                {
                    // null productCode - use default
                    // ReSharper disable once AccessToDisposedClosure
                    Action b1 = () => c.GetValue(1, new List<string> { Feature1Code })
                        .WaitAndUnwrapException()
                        .Should()
                        .Equal(new Dictionary<string, string> { { Feature1Code, null } });
                    b1.Should().NotThrow();
                }

                // invalid productCode
                // ReSharper disable once AccessToDisposedClosure
                Action b2 = () =>
                    {
                        using (var c = server.CreateClient("  ", true))
                        {
                        }
                    };
                b2.Should().Throw<ArgumentException>();

                using (var c = server.CreateClient("unknown", true))
                {
                    // invalid productCode
                    // ReSharper disable once AccessToDisposedClosure
                    Action b3 = () => c.GetValue(1, new List<string> { Feature1Code })
                        .WaitAndUnwrapException()
                        .Should()
                        .BeNull();
                    b3.Should().Throw<ProductCodeNotFoundException>();
                }

                using (var c = server.CreateClient(null, true))
                {
                    // invalid user id
                    // ReSharper disable once AccessToDisposedClosure
                    Action a1 = () =>
                        c.GetValue(0u, new List<string> { Feature1Code })
                            .WaitAndUnwrapException()
                            .Should()
                            .BeNull();
                    a1.Should().Throw<ArgumentException>();


                    // invalid feature codes - null
                    // ReSharper disable once AccessToDisposedClosure
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Action a2 = () => c.GetValue(1, null).WaitAndUnwrapException();
                    a2.Should().Throw<ArgumentException>();

                    // invalid feature codes - empty
                    // ReSharper disable once AccessToDisposedClosure
                    Action a3 = () => c.GetValue(1, new List<string>()).WaitAndUnwrapException();
                    a3.Should().Throw<ArgumentException>();

                    // invalid feature codes - contains null
                    // ReSharper disable once AccessToDisposedClosure
                    Action a4 = () => c.GetValue(1, new List<string> { Feature1Code, null }).WaitAndUnwrapException();
                    a4.Should().Throw<ArgumentException>();

                    // invalid feature codes - contains whitespace
                    // ReSharper disable once AccessToDisposedClosure
                    Action a5 = () => c.GetValue(1, new List<string> { Feature1Code, " " }).WaitAndUnwrapException();
                    a5.Should().Throw<ArgumentException>();
                }
            }
        }

        [Test]
        public void TestGetValueUnknownFeatureCode()
        {
            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);
                        dboh.AddFeature(Feature1Code);
                    });

            using (var server = new FeatureServiceTestServerHelper())
            using (var c = server.CreateClient())
            {
                // ReSharper disable once AccessToDisposedClosure
                Action a1 = () => c.GetValue(1, new List<string> { "unknownCode" }).WaitAndUnwrapException();
                a1.Should().Throw<Exception>().WithMessage("Feature info not found for codes ['unknownCode']");

                // ReSharper disable once AccessToDisposedClosure
                Action a2 = () => c.GetValue(1, new List<string> { Feature1Code, "unknownCode" }).WaitAndUnwrapException();
                a2.Should().Throw<Exception>().WithMessage("Feature info not found for codes ['unknownCode']");
            }
        }

        [Test]
        public async Task TestGetValueNull()
        {
            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);
                        dboh.AddFeature(Feature1Code);
                    });

            using (var server = new FeatureServiceTestServerHelper())
            using (var c = server.CreateClient(null, true, null, true, true))
            {
                (await c.GetValue(1, new List<string> { Feature1Code }))
                    .Should()
                    .Equal(new Dictionary<string, string> { { Feature1Code, null } });
            }
        }

        [Test]
        public async Task TestGetValue()
        {
            var customerId = 0u;
            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        var featureId = dboh.AddFeature(Feature1Code);
                        customerId = (uint)dboh.AddCustomer("customer1");
                        dboh.AddCustomerFeatureValue((int)customerId, featureId, "test", null);
                    });

            using (var server = new FeatureServiceTestServerHelper())
            using (var c = server.CreateClient(null, true, null, true, true))
            {
                (await c.GetValue(customerId, new List<string> { Feature1Code }))
                    .Should()
                    .Equal(new Dictionary<string, string> { { Feature1Code, "test" } });
            }
        }

        [Test]
        public async Task TestGetMultipleValues()
        {
            var customerId = 0u;
            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        var feature1Id = dboh.AddFeature(Feature1Code);
                        var feature2Id = dboh.AddFeature(Feature2Code);

                        customerId = (uint)dboh.AddCustomer("customer1");
                        dboh.AddCustomerFeatureValue((int)customerId, feature1Id, "test1", null);
                        dboh.AddCustomerFeatureValue((int)customerId, feature2Id, "test2", null);
                    });

            using (var server = new FeatureServiceTestServerHelper())
            using (var c = server.CreateClient(null, true, null, true, true))
            {
                (await c.GetValue(customerId, new List<string> { Feature1Code, Feature2Code }))
                    .Should()
                    .Equal(new Dictionary<string, string> { { Feature1Code, "test1" }, { Feature2Code, "test2" } });

                (await c.GetValue(
                        customerId,
                        new List<string> { Feature1Code, Feature2Code, Feature1Code, Feature2Code }))
                    .Should()
                    .Equal(new Dictionary<string, string> { { Feature1Code, "test1" }, { Feature2Code, "test2" } });
            }
        }

        [Test]
        public void TestGetValueFormatError()
        {
            var customerId = 0u;
            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        var featureId = dboh.AddFeature(Feature1Code, FeatureValueAggregationMethod.Sum);
                        customerId = (uint)dboh.AddCustomer("customer1");
                        var serviceId = dboh.AddService("service1");
                        dboh.AddServiceFeatureValue(serviceId, featureId, "test");
                        dboh.AddServiceSubscription(serviceId, (int)customerId);
                    });

            using (var server = new FeatureServiceTestServerHelper())
            using (var c = server.CreateClient(null, true, null, true, true))
            {
                // ReSharper disable once AccessToDisposedClosure
                Action a = () => c.GetValue(customerId, new List<string> { Feature1Code }).WaitAndUnwrapException();
                a.Should().Throw<Exception>().WithMessage("Feature value can't be parsed as a number. userId=1, featureCode=testFeature1");
            }
        }

        [Test]
        public async Task TestInvalidServer()
        {
            var userId = 0u;
            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);
                        var featureId = dboh.AddFeature(Feature1Code);
                        userId = (uint)dboh.AddCustomer("customer1");
                        dboh.AddCustomerFeatureValue((int)userId, featureId, "value1", null);
                    });

            using (var server = new FeatureServiceTestServerHelper())
            {
                var settings = new TestFeatureServiceClientSettings(
                    DatabaseHelper.TestProductCode,
                    new[]
                        {
                            new Uri("http://192.168.0.128/"),
                            server.Uri,
                        },
                    0);
                var nowProvider = new DefaultNowProvider();
                var c = new FeatureServiceClient(settings, null, nowProvider, null, true, null, true, true);
                (await c.GetValue(userId, new List<string> { Feature1Code }))
                    .Should().Equal(new Dictionary<string, string> { { Feature1Code, "value1" } });
            }
        }
    }
}