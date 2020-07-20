using System;
using System.Collections.Generic;
using System.Linq;
using Com.O2Bionics.FeatureService.Impl;
using Com.O2Bionics.FeatureService.Impl.DataModel;
using FluentAssertions;
using NUnit.Framework;

namespace Com.O2Bionics.FeatureService.Tests
{
    [TestFixture]
    public class FeaturesManagerTests
    {
        protected const string Feature1Code = "testFeature1";
        protected const string Feature2Code = "testFeature2";
        protected const string Feature3Code = "testFeature3";

        private readonly DatabaseHelper m_dbh = new DatabaseHelper(new FeatureServiceTestSettings());

        private IFeaturesManager CreateFeaturesManager()
        {
            return new FeaturesManager(m_dbh.Settings, m_dbh.DbFactory, null);
        }

        private string GetFeatureValue(int userId, string featureCode)
        {
            var fm = CreateFeaturesManager();
            var values = fm.GetFeatureValue(DatabaseHelper.TestProductCode, userId, new HashSet<string> { featureCode });
            values.Should().NotBeNull().And.NotBeEmpty();
            return values.Values.Single();
        }

        [SetUp]
        public void SetUp()
        {
            m_dbh.CleanupDatabase();
        }

        [Test]
        public void TestFeatureValueInvalidArguments()
        {
            var customer1Id = 0;

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        dboh.AddFeature(Feature1Code);

                        customer1Id = dboh.AddCustomer("customer1");
                    });

            // invalid productCode
            Action c1 = () => CreateFeaturesManager().GetFeatureValue(null, customer1Id, new HashSet<string> { Feature1Code });
            c1.Should().Throw<ArgumentException>();

            Action c2 = () => CreateFeaturesManager().GetFeatureValue("  ", customer1Id, new HashSet<string> { Feature1Code });
            c2.Should().Throw<ArgumentException>();

            // invalid productCode - unknown code
            Action c3 = () => CreateFeaturesManager().GetFeatureValue("unknown", customer1Id, new HashSet<string> { Feature1Code });
            c3.Should().Throw<ProductCodeNotFoundException>()
                .WithMessage("Can't find connection string for product code 'unknown'");


            // invalid userid
            Action a1 = () => GetFeatureValue(-1, Feature1Code);
            a1.Should().Throw<ArgumentException>();

            // unknown userid
            GetFeatureValue(100, Feature1Code).Should().BeNull();


            // invalid feature codes - null
            Action b1 = () => CreateFeaturesManager().GetFeatureValue(DatabaseHelper.TestProductCode, customer1Id, null);
            b1.Should().Throw<ArgumentNullException>();

            // invalid feature codes - empty
            Action b2 = () => CreateFeaturesManager().GetFeatureValue(DatabaseHelper.TestProductCode, customer1Id, new HashSet<string>());
            b2.Should().Throw<ArgumentException>();

            // invalid feature code - contains null
            Action a2 = () => GetFeatureValue(customer1Id, null);
            a2.Should().Throw<ArgumentException>();

            // invalid feature code - contains whitespace
            Action a3 = () => GetFeatureValue(customer1Id, " ");
            a3.Should().Throw<ArgumentException>();

            // unknown feature code
            Action a4 = () => GetFeatureValue(customer1Id, "unknownCode");
            a4.Should().Throw<FeatureInfoNotFoundException>();

            // feature value is not defined
            GetFeatureValue(customer1Id, Feature1Code).Should().BeNull();
        }

        [Test]
        public void TestFeatureCodeIsCaseInsensitive()
        {
            var customer1Id = 0;

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        customer1Id = dboh.AddCustomer("customer1");

                        var feature1Id = dboh.AddFeature(Feature1Code);

                        dboh.AddCustomerFeatureValue(customer1Id, feature1Id, "value11", null);
                    });

            GetFeatureValue(customer1Id, Feature1Code).Should().Be("value11");
            GetFeatureValue(customer1Id, Feature1Code.ToUpper()).Should().Be("value11");
            GetFeatureValue(customer1Id, Feature1Code.ToLower()).Should().Be("value11");
        }

        [Test]
        public void TestFeatureValueCustomerFeatureValue()
        {
            var customer1Id = 0;

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        customer1Id = dboh.AddCustomer("customer1");

                        var feature1Id = dboh.AddFeature(Feature1Code);
                        var feature2Id = dboh.AddFeature(Feature2Code);
                        var feature3Id = dboh.AddFeature(Feature3Code);

                        dboh.AddCustomerFeatureValue(customer1Id, feature1Id, "value11", null);
                        dboh.AddCustomerFeatureValue(customer1Id, feature2Id, "value12", DateTime.UtcNow.AddYears(1));
                        dboh.AddCustomerFeatureValue(customer1Id, feature3Id, "value13", DateTime.UtcNow.AddYears(-1));
                    });

            // feature value is defined
            GetFeatureValue(customer1Id, Feature1Code).Should().Be("value11");

            // feature value is defined with expiration date
            GetFeatureValue(customer1Id, Feature2Code).Should().Be("value12");

            // feature value is defined with expiration date, expired
            GetFeatureValue(customer1Id, Feature3Code).Should().BeNull();
        }

        [Test]
        public void TestFeatureValueCustomerFeatureValueRequiredService()
        {
            var customer1Id = 0;
            var service1Id = 0;
            var subscriptionSkey = 0;

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        customer1Id = dboh.AddCustomer("customer1");
                        service1Id = dboh.AddService("service1");

                        var feature1Id = dboh.AddFeature(Feature1Code);

                        dboh.AddCustomerFeatureValue(customer1Id, feature1Id, "value11", null, service1Id);
                    });

            // feature value is defined with required service, no service subscriptions added
            GetFeatureValue(customer1Id, Feature1Code).Should().BeNull();


            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);
                        subscriptionSkey = dboh.AddServiceSubscription(service1Id, customer1Id);
                    });

            // service subscription added
            GetFeatureValue(customer1Id, Feature1Code).Should().Be("value11");


            m_dbh.Query(
                db => { db.Execute("update service_subscription set is_deleted=1 where SUBSCRIPTION_SKEY=" + subscriptionSkey); });

            // service subscription marked as deleted
            GetFeatureValue(customer1Id, Feature1Code).Should().BeNull();
        }

        [Test]
        public void TestFeatureValueSubscriptionValue()
        {
            var customer1Id = 0;
            var service1Id = 0;
            var subscriptionSkey = 0;

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        customer1Id = dboh.AddCustomer("customer1");

                        var feature1Id = dboh.AddFeature(Feature1Code);
                        service1Id = dboh.AddService("service1", 0);
                        dboh.AddServiceFeatureValue(service1Id, feature1Id, "value11");
                    });

            // no service subscriptions
            GetFeatureValue(customer1Id, Feature1Code).Should().BeNull();


            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);
                        subscriptionSkey = dboh.AddServiceSubscription(service1Id, customer1Id);
                    });

            // service subscription added
            GetFeatureValue(customer1Id, Feature1Code).Should().Be("value11");


            m_dbh.Query(
                db => { db.Execute("update service_subscription set is_deleted=1 where SUBSCRIPTION_SKEY=" + subscriptionSkey); });

            // service subscription marked as deleted
            GetFeatureValue(customer1Id, Feature1Code).Should().BeNull();
        }

        // am: null, CAS, Sum, Min, Max
        // value: number/not a number
        // qty: null, 1, 7
        // use qty: null, true, false
        // type: null, addon, service

        public static readonly FeatureValueAggregationMethod?[] FeatureValueAggregationMethods =
            {
                null,
                FeatureValueAggregationMethod.CAS,
                FeatureValueAggregationMethod.Sum,
                FeatureValueAggregationMethod.Min,
                FeatureValueAggregationMethod.Max,
            };

        [Test]
        public void TestFeatureValueSubscriptionValueAggregationModeNumericValues(
            [ValueSource("FeatureValueAggregationMethods")] FeatureValueAggregationMethod? aggregationMethod,
            [Values(null, true, false)] bool? useQuantity)
        {
            var customer1Id = 0;

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        customer1Id = dboh.AddCustomer("customer1");

                        var feature1Id = dboh.AddFeature(Feature1Code, aggregationMethod, useQuantity);

                        var plan1Id = dboh.AddService("plan1", 0);
                        var addon1Id = dboh.AddService("addon1", 1);
                        var addon2Id = dboh.AddService("addon2", null);
                        var addon3Id = dboh.AddService("addon3", 1);

                        dboh.AddServiceFeatureValue(plan1Id, feature1Id, "2");
                        dboh.AddServiceFeatureValue(addon1Id, feature1Id, "5");
                        dboh.AddServiceFeatureValue(addon2Id, feature1Id, "3");
                        dboh.AddServiceFeatureValue(addon3Id, feature1Id, "7");

                        dboh.AddServiceSubscription(plan1Id, customer1Id, 2);
                        dboh.AddServiceSubscription(addon1Id, customer1Id);
                        dboh.AddServiceSubscription(addon2Id, customer1Id, 9);
                        dboh.AddServiceSubscription(addon3Id, customer1Id, 3);
                    });

            var useQuantityValue = useQuantity != null && useQuantity.Value;
            string expectedValue = null;
            switch (aggregationMethod)
            {
                case null:
                case FeatureValueAggregationMethod.CAS:
                    expectedValue = useQuantityValue ? "21" : "7";
                    break;
                case FeatureValueAggregationMethod.Min:
                    expectedValue = useQuantityValue ? "4" : "2";
                    break;
                case FeatureValueAggregationMethod.Max:
                    expectedValue = useQuantityValue ? "27" : "7";
                    break;
                case FeatureValueAggregationMethod.Sum:
                    expectedValue = useQuantityValue ? "57" : "17";
                    break;
            }

            GetFeatureValue(customer1Id, Feature1Code).Should().Be(expectedValue);
        }

        [Test]
        public void TestFeatureValueSubscriptionValueAggregationModeStringValues(
            [ValueSource("FeatureValueAggregationMethods")] FeatureValueAggregationMethod? aggregationMethod,
            [Values(null, true, false)] bool? useQuantity)
        {
            var customer1Id = 0;

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        customer1Id = dboh.AddCustomer("customer1");

                        var feature1Id = dboh.AddFeature(Feature1Code, aggregationMethod, useQuantity);

                        var plan1Id = dboh.AddService("plan1", 0);
                        var addon1Id = dboh.AddService("addon1", 1);
                        var addon2Id = dboh.AddService("addon2", null);
                        var addon3Id = dboh.AddService("addon3", 1);

                        dboh.AddServiceFeatureValue(plan1Id, feature1Id, "test2");
                        dboh.AddServiceFeatureValue(addon1Id, feature1Id, "test1");
                        dboh.AddServiceFeatureValue(addon2Id, feature1Id, "test3");
                        dboh.AddServiceFeatureValue(addon3Id, feature1Id, "test4");

                        dboh.AddServiceSubscription(plan1Id, customer1Id);
                        dboh.AddServiceSubscription(addon1Id, customer1Id);
                        dboh.AddServiceSubscription(addon2Id, customer1Id);
                        dboh.AddServiceSubscription(addon3Id, customer1Id);
                    });

            var useQuantityValue = useQuantity != null && useQuantity.Value;
            string expectedValue = null;
            var expectedFail = false;
            switch (aggregationMethod)
            {
                case null:
                case FeatureValueAggregationMethod.CAS:
                    expectedFail = useQuantityValue;
                    if (!useQuantityValue)
                        expectedValue = "test4";
                    break;
                case FeatureValueAggregationMethod.Min:
                case FeatureValueAggregationMethod.Max:
                case FeatureValueAggregationMethod.Sum:
                    expectedFail = true;
                    break;
            }

            Func<string> call = () => GetFeatureValue(customer1Id, Feature1Code);
            if (expectedFail)
                ((Action)(() => call())).Should().Throw<FeatureValueFormatException>();
            else
                call().Should().Be(expectedValue);
        }

        [Test]
        public void TestFeatureValueDoesntThrowForNotANumberUserOverride()
        {
            var customer1Id = 0;
            var feature1Id = 0;

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        customer1Id = dboh.AddCustomer("customer1");
                        feature1Id = dboh.AddFeature(Feature1Code, FeatureValueAggregationMethod.Sum);
                        var plan1Id = dboh.AddService("plan1");

                        // service has not a number value
                        dboh.AddServiceFeatureValue(plan1Id, feature1Id, "test2");
                        dboh.AddServiceSubscription(plan1Id, customer1Id);
                    });

            Action a1 = () => GetFeatureValue(customer1Id, Feature1Code);

            a1.Should().Throw<FeatureValueFormatException>();

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        // user override also has not a number value
                        dboh.AddCustomerFeatureValue(customer1Id, feature1Id, "test3", null);
                    });

            a1.Should().NotThrow();
            GetFeatureValue(customer1Id, Feature1Code).Should().Be("test3");
        }

        [Test]
        public void TestFeatureValueMultipleFeatureCodeSubscription()
        {
            var customer1Id = 0;

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        customer1Id = dboh.AddCustomer("customer1");
                        var feature1Id = dboh.AddFeature(Feature1Code);
                        var feature2Id = dboh.AddFeature(Feature2Code);
                        dboh.AddFeature(Feature3Code);

                        var plan1Id = dboh.AddService("plan1");

                        dboh.AddServiceFeatureValue(plan1Id, feature1Id, "test2");
                        dboh.AddServiceFeatureValue(plan1Id, feature2Id, "test3");

                        dboh.AddServiceSubscription(plan1Id, customer1Id);
                    });

            CreateFeaturesManager()
                .GetFeatureValue(DatabaseHelper.TestProductCode, customer1Id, new HashSet<string> { Feature1Code, Feature2Code })
                .Should().Equal(
                    new Dictionary<string, string>
                        {
                                { Feature1Code, "test2" },
                                { Feature2Code, "test3" },
                        });

            CreateFeaturesManager()
                .GetFeatureValue(
                    DatabaseHelper.TestProductCode,
                    customer1Id,
                    new HashSet<string> { Feature1Code, Feature2Code, Feature1Code, Feature2Code })
                .Should().Equal(
                    new Dictionary<string, string>
                        {
                                { Feature1Code, "test2" },
                                { Feature2Code, "test3" },
                        });

            CreateFeaturesManager()
                .GetFeatureValue(DatabaseHelper.TestProductCode, customer1Id, new HashSet<string> { Feature1Code, Feature2Code, Feature3Code })
                .Should().Equal(
                    new Dictionary<string, string>
                        {
                                { Feature1Code, "test2" },
                                { Feature2Code, "test3" },
                                { Feature3Code, null },
                        });
        }

        [Test]
        public void TestFeatureValueMultipleFeatureCodeOverride()
        {
            var customer1Id = 0;

            m_dbh.Query(
                db =>
                    {
                        var dboh = new DatabaseObjectHelper(db);

                        customer1Id = dboh.AddCustomer("customer1");
                        var feature1Id = dboh.AddFeature(Feature1Code);
                        var feature2Id = dboh.AddFeature(Feature2Code);
                        dboh.AddFeature(Feature3Code);

                        var plan1Id = dboh.AddService("plan1");

                        dboh.AddServiceFeatureValue(plan1Id, feature1Id, "test2");
                        dboh.AddServiceFeatureValue(plan1Id, feature2Id, "test3");

                        dboh.AddServiceSubscription(plan1Id, customer1Id);

                        dboh.AddCustomerFeatureValue(customer1Id, feature2Id, "test4", null);
                    });

            CreateFeaturesManager()
                .GetFeatureValue(
                    DatabaseHelper.TestProductCode,
                    customer1Id,
                    new HashSet<string> { Feature1Code, Feature2Code, Feature3Code },
                    null,
                    null,
                    true,
                    false)
                .Should().Equal(
                    new Dictionary<string, string>
                        {
                                { Feature1Code, "test2" },
                                { Feature2Code, "test4" },
                                { Feature3Code, null },
                        });
        }
    }
}