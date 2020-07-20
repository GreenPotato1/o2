using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.PageTracker.Storage;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Com.O2Bionics.PageTracker.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public sealed class PageTrackerUnitTests
    {
        private const int TimeoutSeconds = 3;

        [Test]
        [Timeout(TimeoutSeconds * 2 * 1000)]
        public async Task TimeoutTest()
        {
            var settings = new JsonSettingsReader().ReadFromFile<PageTrackerSettings>();
            settings.AddBufferSize = 3;
            settings.AddBufferFlushTimeout = TimeSpan.FromSeconds(TimeoutSeconds);

            var ipAddressResolver = Substitute.For<IGeoIpAddressResolver>();
            var userAgentParser = Substitute.For<IUserAgentParser>();

            var actual = new List<PageView>();
            var esClient = Substitute.For<IEsClient>();
            esClient.IndexMany(Arg.Any<string>(), Arg.Any<List<PageView>>()).Returns(
                c =>
                    {
                        var arg = c.Arg<List<PageView>>();
                        if (null != arg)
                            actual.AddRange(arg);
                        return null;
                    });

            var idGenerator = Substitute.For<IIdGenerator>();
            var nowProvider = new DefaultNowProvider();

            var featureServiceClient = Substitute.For<IFeatureServiceClient>();
            featureServiceClient.GetValue(TestConstants.CustomerId, Arg.Any<List<string>>()).Returns(
                c =>
                    {
                        var features = c.Arg<List<string>>();
                        if (null == features || 1 != features.Count || FeatureCodes.IsGeoLocationEnabled != features[0])
                            throw new Exception("Unknown feature.");

                        var r = new Dictionary<string, string> { { FeatureCodes.IsGeoLocationEnabled, FeatureValues.False } };
                        return Task.FromResult(r);
                    }
            );

            using (var pageTracker = new PageTrackerEs(
                ipAddressResolver,
                userAgentParser,
                settings,
                esClient,
                idGenerator,
                featureServiceClient))
            {
                var expected = new List<PageView>();

                var now = nowProvider.UtcNow;
                var url = new Uri("https://abc.def/f/g/h");
                var timeZoneDescription = new TimeZoneDescription(100, "Time ion");

                for (var i = 1; i < settings.AddBufferSize; i++)
                {
                    var addRecordArgs = new AddRecordArgs
                        {
                            CustomerId = TestConstants.CustomerId,
                            VisitorId = (ulong)(TestConstants.CustomerId + i),
                            CustomText = "Some test" + i,
                            Url = url,
                            TimeZone = timeZoneDescription,
                            UserAgentString = TestConstants.UserAgent,
                            Ip = new IPAddress(34567356),
                        };
                    expected.Add(
                        new PageView
                            {
                                CustomerId = addRecordArgs.CustomerId,
                                VisitorId = (long)addRecordArgs.VisitorId,
                                CustomText = addRecordArgs.CustomText,
                                Id = "0",
                                IpAddress = "188.116.15.2",
                                TimeZone = addRecordArgs.TimeZone,
                                UriInfo = new UriInfo
                                    {
                                        Host = "abc.def",
                                        Path = "/f/g/h",
                                        Port = 443,
                                        Scheme = "https",
                                        Url = url.AbsoluteUri,
                                    },
                                Timestamp = now,
                            });
                    await pageTracker.Add(now, addRecordArgs);
                }

                var now2 = nowProvider.UtcNow;
                var diff = now2 - now;
                Assert.LessOrEqual(
                    diff.TotalMilliseconds,
                    500,
                    $"{nameof(PageTrackerEs)}.{nameof(PageTrackerEs.Add)} should not take long milliseconds");

                const int halfPeriodMs = TimeoutSeconds * 1000 / 2;
                Thread.Sleep(halfPeriodMs);
                Assert.AreEqual(settings.AddBufferSize - 1, pageTracker.QueueSize, "Queue size, shortly after adding.");

                Thread.Sleep(halfPeriodMs + 1000);
                Assert.AreEqual(0, pageTracker.QueueSize, $"The queue must have been emptied after {TimeoutSeconds + 1} seconds.");

                actual.Should().BeEquivalentTo(expected, "Added records");
            }
        }
    }
}