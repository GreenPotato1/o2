using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.PageTracker.DataModel;
using Com.O2Bionics.PageTracker.Storage;
using Com.O2Bionics.PageTracker.Tests.Settings;
using Com.O2Bionics.PageTracker.Utilities;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace Com.O2Bionics.PageTracker.Tests.Performance
{
    [TestFixture]
    [Explicit]
    public sealed class WriteReadTestsEs : WriteReadTestsBase
    {
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

            var result = new PageTrackerEs(
                Resolver,
                UserAgentParser,
                Settings,
                esClient,
                generator,
                FeatureServiceClient);
            return result;
        }
    }

    [TestFixture]
    [Explicit]
    public sealed class WriteReadTestsMySql : WriteReadTestsBase
    {
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

            return new PageTrackerMySql(
                Resolver,
                UserAgentParser,
                dbFactory,
                FeatureServiceClient);
        }
    }

    public abstract class WriteReadTestsBase : PageTrackerTestsBase, IDisposable
    {
        private const int ThreadCount = 20;
        private const int ReadItemCount = 500;
        private const int WriteItemCount = ReadItemCount * 5;
        private const int IdStorageBlockSize = 1000;
        private const int AddBufferSize = 1000;
        private static readonly Level m_testLogLevel = Level.Error;

        protected readonly DefaultNowProvider NowProvider = new DefaultNowProvider();
        protected readonly UserAgentParser UserAgentParser = new UserAgentParser();
        protected readonly IFeatureServiceClient FeatureServiceClient = Substitute.For<IFeatureServiceClient>();
        protected MaxMindLocalGeoIpAddressResolver Resolver;

        protected override TestPageTrackerSettings Settings =>
            new TestPageTrackerSettings
                {
                    IdStorageBlockSize = IdStorageBlockSize,
                    AddBufferSize = AddBufferSize,
                    AddBufferFlushTimeout = TimeSpan.FromSeconds(5),
                };

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            Resolver = new MaxMindLocalGeoIpAddressResolver(Settings);

            FeatureServiceClient
                .GetValue(TestConstants.CustomerId, Arg.Any<List<string>>())
                .Returns(
                    s =>
                        {
                            var features = s.Arg<List<string>>();
                            if (features.Count == 1 && features[0] == FeatureCodes.IsGeoLocationEnabled)
                                return new Dictionary<string, string> { { FeatureCodes.IsGeoLocationEnabled, FeatureValues.False } };
                            return null;
                        });
        }

        [SetUp]
        public void SetUp()
        {
            SetUpStorage();
        }

        protected abstract void SetUpStorage();
        protected abstract void FlushStorage([NotNull] IPageTracker pageTracker);
        protected abstract IPageTracker CreateService();


        [Test]
        public void UniqueVisitorsTest([Values(0, 1, 2)] int repeat)
        {
            using (var pageTracker = CreateService())
            {
                var createdVisitors = new ConcurrentHashSet<ulong>();

                var writeThreads = new BackgroundThreads(
                    ThreadCount,
                    async threadIndex =>
                        {
                            var random = new Random(threadIndex);
                            for (var i = 0; i < WriteItemCount; i++)
                            {
                                var request = AddRecordArgsGenerator.GetNext(0, random);
                                var result = await pageTracker.Add(NowProvider.UtcNow, request);
                                result.Should().NotBeNull();
                                result.VisitorId.Should().BeGreaterThan(0);

                                createdVisitors.TryAdd(result.VisitorId).Should().BeTrue();
                            }
                        });
                LogHelper.WithLogLevel(
                    m_testLogLevel,
                    () => writeThreads.Measure(GetType().Name + "." + nameof(UniqueVisitorsTest), "write", WriteItemCount));

                FlushStorage(pageTracker);

                var createdVisitorArray = createdVisitors.KeysToArray();
                var readThreads = new BackgroundThreads(
                    ThreadCount,
                    async threadIndex =>
                        {
                            var random = new Random(threadIndex);
                            for (var i = 0; i < ReadItemCount; i++)
                            {
                                var visitorId = createdVisitorArray[random.Next(createdVisitorArray.Length)];
                                var response = await pageTracker.Get(TestConstants.CustomerId, visitorId);
                                response.Should().NotBeNull();
                                response.Visitor.Should().NotBeNull();
                                response.Items.Should().HaveCount(1);
                            }
                        });
                LogHelper.WithLogLevel(
                    m_testLogLevel,
                    () => readThreads.Measure(GetType().Name + "." + nameof(UniqueVisitorsTest), "read", ReadItemCount));
            }
        }

        [Test]
        public void SameVisitorTest([Values(1, 2, 3)] int repeat)
        {
            using (var pageTracker = CreateService())
            {
                var addVisitorRequest = AddRecordArgsGenerator.GetNext(0);
                var addVisitorResponse = pageTracker
                    .Add(NowProvider.UtcNow, addVisitorRequest)
                    .WaitAndUnwrapException();
                addVisitorResponse.VisitorId.Should().BeGreaterThan(0);

                var visitorId = addVisitorResponse.VisitorId;

                var writeThreads = new BackgroundThreads(
                    ThreadCount,
                    async threadIndex =>
                        {
                            var random = new Random(threadIndex);
                            for (var i = 0; i < WriteItemCount; i++)
                            {
                                var request = AddRecordArgsGenerator.GetNext(visitorId, random);
                                var response = await pageTracker.Add(NowProvider.UtcNow, request);
                                response.Should().NotBeNull();
                                response.VisitorId.Should().Be(visitorId);
                            }
                        });
                LogHelper.WithLogLevel(
                    m_testLogLevel,
                    () => writeThreads.Measure(GetType().Name + "." + nameof(SameVisitorTest), "write", WriteItemCount));

                FlushStorage(pageTracker);

                var readThreads = new BackgroundThreads(
                    ThreadCount,
                    async threadIndex =>
                        {
                            for (var i = 0; i < ReadItemCount; i++)
                            {
                                var response = await pageTracker.Get(addVisitorRequest.CustomerId, visitorId);
                                response.Should().NotBeNull();
                                response.Visitor.Should().NotBeNull();
                                response.Items.Should().NotBeEmpty();
                            }
                        });
                LogHelper.WithLogLevel(
                    m_testLogLevel,
                    () => readThreads.Measure(GetType().Name + "." + nameof(SameVisitorTest), "read", ReadItemCount));
            }
        }

        private static class AddRecordArgsGenerator
        {
            private static readonly IPAddress[] m_addresses = Enumerable.Range(1, 254)
                .Select(x => IPAddress.Parse("137.18.44." + x))
                .ToArray();

            private static readonly TimeZoneDescription[] m_timezones = Enumerable.Range(1, 30)
                .Select(x => new TimeZoneDescription(x, "time zone " + x))
                .ToArray();

            private static readonly string[] m_userAgents = Enumerable.Range(1, 20)
                .Select(x => "amaya/9.52 libwww/5.4.0." + x)
                .ToArray();

            private static readonly Uri[] m_urls = Enumerable.Range(1, 20)
                .SelectMany(x => Enumerable.Range(1, 15).Select(y => new Uri("http://site" + x + ".com/page" + y + ".aspx?test1=apple")))
                .ToArray();

            public static AddRecordArgs GetNext(ulong visitorId, Random random = null)
            {
                return new AddRecordArgs
                    {
                        CustomerId = TestConstants.CustomerId,
                        Ip = m_addresses[random?.Next(m_addresses.Length) ?? 0],
                        TimeZone = m_timezones[random?.Next(m_timezones.Length) ?? 0],
                        UserAgentString = m_userAgents[random?.Next(m_userAgents.Length) ?? 0],
                        Url = m_urls[random?.Next(m_urls.Length) ?? 0],
                        CustomText = "some custom params",
                        VisitorId = visitorId,
                        VisitorExternalId = "visitor " + visitorId,
                    };
            }
        }

        public void Dispose()
        {
            Resolver.Dispose();
        }
    }
}