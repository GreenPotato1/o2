using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.PageTracker.DataModel;
using Com.O2Bionics.PageTracker.Storage;
using Com.O2Bionics.PageTracker.Tests.Utilities;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using NUnit.Framework;
using GeoLocation = Com.O2Bionics.PageTracker.Contract.GeoLocation;

namespace Com.O2Bionics.PageTracker.Tests
{
    [TestFixture]
    public sealed class FunctionalTestsEs : FunctionalTestsBase
    {
        protected override bool ShallCacheGeoLocation => false;

        protected override void SetUpStorage()
        {
            PageTrackerIndexHelper.DeleteIndices(Settings);
            PageTrackerIndexHelper.CreateIndices(Settings);
        }

        protected override void FlushStorage(IPageTracker pageTracker)
        {
            pageTracker.Flush();

            var client = new EsClient(Settings.ElasticConnection);
            client.Flush(Settings.PageVisitIndex.Name);
        }

        protected override IPageTracker CreateService()
        {
            var esClient = new EsClient(Settings.ElasticConnection);
            var idStorage = new IdStorage(Settings, esClient);
            var generator = new IdGenerator(idStorage);
            return new PageTrackerEs(
                Resolver,
                UserAgentParser,
                Settings,
                esClient,
                generator,
                FeatureServiceClient);
        }

        protected override DateTime UtcNow => DateTime.UtcNow;

        [Test]
        public async Task TestPaging()
        {
            const int hits = 10;
            const int pageSize = 4;

            using (var tracker = CreateService())
            {
                var pageViews = new List<string>();
                ulong visitorId = 0;
                for (var i = 0; i < hits; i++)
                {
                    var addResult = await tracker.Add(UtcNow, CreateAddParams(visitorId));
                    visitorId = addResult.VisitorId;
                    pageViews.Add(addResult.PageHistoryId);
                }

                FlushStorage(tracker);

                pageViews.Reverse();

                var getResult = await tracker.Get(TestConstants.CustomerId, visitorId, hits + 1);
                ShouldBeEqual(getResult, pageViews, false);

                getResult = await tracker.Get(TestConstants.CustomerId, visitorId, hits);
                ShouldBeEqual(getResult, pageViews, false);

                getResult = await tracker.Get(TestConstants.CustomerId, visitorId, hits - 1);
                ShouldBeEqual(getResult, pageViews.Take(hits - 1), true);

                getResult = await tracker.Get(TestConstants.CustomerId, visitorId, hits - 1, getResult.SearchPosition);
                ShouldBeEqual(getResult, pageViews.Skip(hits - 1).Take(hits - 1), false);


                getResult = await tracker.Get(TestConstants.CustomerId, visitorId, pageSize);
                ShouldBeEqual(getResult, pageViews.Take(pageSize), true);

                getResult = await tracker.Get(TestConstants.CustomerId, visitorId, pageSize, getResult.SearchPosition);
                ShouldBeEqual(getResult, pageViews.Skip(pageSize).Take(pageSize), true);

                getResult = await tracker.Get(TestConstants.CustomerId, visitorId, pageSize, getResult.SearchPosition);
                ShouldBeEqual(getResult, pageViews.Skip(pageSize * 2).Take(pageSize), false);
            }
        }

        private static void ShouldBeEqual(GetHistoryResult result, IEnumerable<string> expectedIds, bool expectedMore)
        {
            // ReSharper disable PossibleMultipleEnumeration
            if (expectedIds.Any())
                result.Items.Should().NotBeNull();
            // ReSharper disable once AssignNullToNotNullAttribute
            result.Items.Select(x => x.Id)
                .Should().BeEquivalentTo(expectedIds, s => s.WithStrictOrderingFor(x => x));
            // ReSharper restore PossibleMultipleEnumeration

            result.HasMore.Should().Be(expectedMore);
            if (expectedMore)
                result.SearchPosition.Should().NotBeNull();
            else
                result.SearchPosition.Should().BeNull();
        }

        [Test]
        public async Task TestVisitorIdMaxValue()
        {
            using (var tracker = CreateService())
            {
                var utcNow = NowProvider.UtcNow;

                const ulong visitorId = ulong.MaxValue;

                var addParams = new AddRecordArgs
                    {
                        VisitorId = visitorId,
                        CustomerId = TestConstants.CustomerId,
                        VisitorExternalId = "some external id",
                        Ip = IPAddress.Parse("192.168.41.12"),
                        TimeZone = new TimeZoneDescription(130, "Time zone 130"),
                        UserAgentString = "Test user agent",
                        Url = new Uri("http://" + TestConstants.CustomerMainDomain + "/test1.php?p1=p2"),
                        CustomText = "test custom text 2"
                    };
                var addResult = await tracker.Add(utcNow, addParams);
                addResult.Should().NotBeNull();
                addResult.VisitorId.Should().Be(visitorId);
                addResult.PageHistoryId.Should().NotBeNullOrWhiteSpace();

                FlushStorage(tracker);

                var getResult = await tracker.Get(TestConstants.CustomerId, addResult.VisitorId);
                getResult.Should().NotBeNull();
                getResult.Items.Should().HaveCount(1);
            }
        }
    }

    [TestFixture]
    public sealed class FunctionalTestsMySql : FunctionalTestsBase
    {
        protected override bool ShallCacheGeoLocation => true;

        protected override void SetUpStorage()
        {
            var databaseManager = new DatabaseManager(Settings.Database, false);
            databaseManager.RecreateSchema();
            databaseManager.ReloadData();
        }

        protected override void FlushStorage(IPageTracker pageTracker)
        {
        }

        protected override IPageTracker CreateService()
        {
            var dbFactory = new DatabaseFactory(Settings);
            return new PageTrackerMySql(Resolver, UserAgentParser, dbFactory, FeatureServiceClient);
        }

        protected override DateTime UtcNow => MySqlDateTime.UtcNow();
    }

    public abstract class FunctionalTestsBase : PageTrackerTestsBase
    {
        private const double Precision = 0.8;
        protected abstract bool ShallCacheGeoLocation { get; }

        private readonly GeoLocation m_locationInfo = new GeoLocation
            {
                Country = "Zimbabwe",
                City = "New Vasyuki",
                Point = new Point
                    {
                        lat = 89.567,
                        lon = 32,
                    },
            };

        protected readonly UserAgentInfo UserAgentInfo = new UserAgentInfo
            {
                Os = "TestOs",
                Device = "TestDevice",
                UserAgent = "TestUserAgent",
                UserAgentString = "TestUserAgentString"
            };

        protected readonly IGeoIpAddressResolver Resolver = Substitute.For<IGeoIpAddressResolver>();
        protected readonly IUserAgentParser UserAgentParser = Substitute.For<IUserAgentParser>();
        protected readonly IFeatureServiceClient FeatureServiceClient = Substitute.For<IFeatureServiceClient>();
        protected readonly DefaultNowProvider NowProvider = new DefaultNowProvider();

        [SetUp]
        public void SetUp()
        {
            Resolver.ResolveAddress(Arg.Any<IPAddress>()).Returns(m_locationInfo);
            UserAgentParser.Parse(Arg.Any<string>()).Returns(UserAgentInfo);
            InitializeFeatureServiceClientMock(true);

            SetUpStorage();
        }

        protected abstract void SetUpStorage();
        protected abstract void FlushStorage([NotNull] IPageTracker pageTracker);

        protected abstract IPageTracker CreateService();

        [Test]
        public async Task TestAddGet()
        {
            using (var tracker = CreateService())
            {
                var utcNow = NowProvider.UtcNow;

                var addParams = CreateAddParams();
                var addResult = await tracker.Add(utcNow, addParams);
                addResult.Should().NotBeNull();
                addResult.VisitorId.Should().BeGreaterThan(0);
                addResult.PageHistoryId.Should().NotBeNullOrWhiteSpace();

                FlushStorage(tracker);

                var getResult = await tracker.Get(TestConstants.CustomerId, addResult.VisitorId);
                getResult.Should().NotBeNull();

                var expectedVisitor = new PageHistoryVisitorInfo
                    {
                        TimestampUtc = utcNow,
                        VisitorExternalId = addParams.VisitorExternalId,
                        IpLocation = m_locationInfo,
                        TimeZone = addParams.TimeZone,
                        UserAgent = UserAgentInfo
                    };
                getResult.Visitor.Should().BeEquivalentTo(
                    expectedVisitor,
                    x => x.Excluding(y => y.Ip).Excluding(y => y.TimestampUtc).CompareWithPrecision(Precision),
                    nameof(getResult.Visitor));

                var expectedItems = new List<PageHistoryRecord>
                    {
                        new PageHistoryRecord
                            {
                                Id = addResult.PageHistoryId,
                                Url = addParams.Url,
                                TimestampUtc = utcNow,
                                CustomText = addParams.CustomText
                            }
                    };
                getResult.Items.Should().BeEquivalentTo(expectedItems, x => x.Excluding(y => y.TimestampUtc));
            }
        }

        protected static AddRecordArgs CreateAddParams(ulong visitorId = 0)
        {
            return new AddRecordArgs
                {
                    VisitorId = visitorId,
                    CustomerId = TestConstants.CustomerId,
                    VisitorExternalId = "some external id",
                    Ip = IPAddress.Parse("192.168.41.12"),
                    TimeZone = new TimeZoneDescription(130, "Time zone 130"),
                    UserAgentString = "Test user agent",
                    Url = new Uri("http://" + TestConstants.CustomerMainDomain + "/test1.php?p1=p2"),
                    CustomText = "test custom text 2",
                };
        }

        [Test]
        public async Task TestOneVisitorTwoPages()
        {
            using (var tracker = CreateService())
            {
                var request1 = CreateAddParams();
                var utcNow1 = NowProvider.UtcNow;
                var addResult1 = await tracker.Add(utcNow1, request1);
                addResult1.Should().NotBeNull();
                addResult1.VisitorId.Should().BeGreaterThan(0);
                addResult1.PageHistoryId.Should().NotBeNullOrWhiteSpace();

                var request2 = new AddRecordArgs
                    {
                        VisitorId = addResult1.VisitorId,

                        CustomerId = TestConstants.CustomerId,
                        VisitorExternalId = "ext id 2",
                        Ip = IPAddress.Parse("137.18.144.51"),
                        TimeZone = new TimeZoneDescription(180, "Test time zone 2"),
                        UserAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.87",
                        Url = new Uri("http://" + TestConstants.CustomerMainDomain + "/test2.php?h1=4#cc"),
                        CustomText = "some custom params 2",
                    };
                var utcNow2 = utcNow1.AddSeconds(20);
                var addResult2 = await tracker.Add(utcNow2, request2);
                addResult2.Should().NotBeNull();
                addResult2.VisitorId.Should().Be(addResult1.VisitorId);
                addResult2.PageHistoryId.Should().NotBeNullOrWhiteSpace().And.NotBe(addResult1.PageHistoryId);

                FlushStorage(tracker);

                var getResult = await tracker.Get(TestConstants.CustomerId, addResult1.VisitorId);
                getResult.Should().NotBeNull();
                var expectedVisitor = new PageHistoryVisitorInfo
                    {
                        TimestampUtc = utcNow2,
                        VisitorExternalId = request2.VisitorExternalId,
                        IpLocation = m_locationInfo,
                        TimeZone = request2.TimeZone,
                        UserAgent = UserAgentInfo
                    };
                getResult.Visitor.Should().BeEquivalentTo(
                    expectedVisitor,
                    x => x.Excluding(y => y.Ip).Excluding(y => y.TimestampUtc).CompareWithPrecision(Precision));

                var expectedItems = new List<PageHistoryRecord>
                    {
                        new PageHistoryRecord
                            {
                                Id = addResult2.PageHistoryId,
                                TimestampUtc = utcNow2,
                                Url = request2.Url,
                                CustomText = request2.CustomText
                            },
                        new PageHistoryRecord
                            {
                                Id = addResult1.PageHistoryId,
                                TimestampUtc = utcNow1,
                                Url = request1.Url,
                                CustomText = request1.CustomText
                            }
                    };
                getResult.Items.Should().BeEquivalentTo(
                    expectedItems,
                    x => x.Excluding(y => y.TimestampUtc));
            }
        }

        protected abstract DateTime UtcNow { get; }

        [Test]
        public async Task TestGeoLocationFeature(
            [Values(true, false)] bool isGeoLocationEnabledForAdd1,
            [Values(true, false)] bool isGeoLocationEnabledForAdd2,
            [Values(true, false)] bool isGeoLocationEnabledForGet)
        {
            using (var tracker = CreateService())
            {
                var addParams = CreateAddParams();

                InitializeFeatureServiceClientMock(isGeoLocationEnabledForAdd1);
                Resolver.ClearReceivedCalls();
                var addResult = await tracker.Add(UtcNow, addParams);
                ResolverShouldResolve(isGeoLocationEnabledForAdd1 ? addParams.Ip : null);

                InitializeFeatureServiceClientMock(isGeoLocationEnabledForAdd2);
                Resolver.ClearReceivedCalls();
                addParams.VisitorId = addResult.VisitorId;
                tracker.Add(UtcNow, addParams).WaitAndUnwrapException();
                var useGeo2 = isGeoLocationEnabledForAdd2 && (!ShallCacheGeoLocation || ShallCacheGeoLocation && !isGeoLocationEnabledForAdd1);
                ResolverShouldResolve(useGeo2 ? addParams.Ip : null);

                FlushStorage(tracker);

                InitializeFeatureServiceClientMock(isGeoLocationEnabledForGet);
                Resolver.ClearReceivedCalls();
                var result = await tracker.Get(TestConstants.CustomerId, addResult.VisitorId);
                ResolverShouldResolve(null);
                result.Should().NotBeNull();
                var shouldIpAddressBeResolved = ShallCacheGeoLocation
                    ? (isGeoLocationEnabledForAdd1 || isGeoLocationEnabledForAdd2) && isGeoLocationEnabledForGet
                    : isGeoLocationEnabledForAdd2 && isGeoLocationEnabledForGet;

                result.Visitor.Should().BeEquivalentTo(
                    new PageHistoryVisitorInfo
                        {
                            VisitorExternalId = addParams.VisitorExternalId,
                            IpLocation = shouldIpAddressBeResolved
                                ? m_locationInfo
                                : null,
                            TimeZone = addParams.TimeZone,
                            UserAgent = UserAgentInfo,
                        },
                    x => x.Excluding(y => y.Ip)
                        .Excluding(y => y.TimestampUtc).CompareWithPrecision(Precision));
            }
        }

        private void ResolverShouldResolve([CanBeNull] IPAddress ip)
        {
            if (ip is null)
                Resolver.DidNotReceiveWithAnyArgs().ResolveAddress(Arg.Any<IPAddress>());
            else
                Resolver.Received().ResolveAddress(ip);
        }

        private void InitializeFeatureServiceClientMock(bool isEnabled)
        {
            FeatureServiceClient
                .GetValue(Arg.Any<uint>(), Arg.Any<List<string>>())
                .Returns(
                    new Dictionary<string, string>
                        {
                            { FeatureCodes.IsGeoLocationEnabled, isEnabled ? FeatureValues.True : FeatureValues.False }
                        });
        }
    }
}