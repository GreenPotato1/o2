using System;
using System.Net;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.PageTracker.Tests.Utilities;
using Com.O2Bionics.PageTracker.Utilities;
using FluentAssertions;
using NUnit.Framework;

namespace Com.O2Bionics.PageTracker.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public sealed class GeoIpAddressResolverTests : PageTrackerTestsBase, IDisposable
    {
        private readonly MaxMindLocalGeoIpAddressResolver m_resolver;

        public GeoIpAddressResolverTests()
        {
            m_resolver = new MaxMindLocalGeoIpAddressResolver(Settings);
        }

        public void Dispose()
        {
            m_resolver.Dispose();
        }

        [Test]
        public void TestGlobalAddress()
        {
            const double precision = 1e-7;

            m_resolver.ResolveAddress(IPAddress.Parse("137.18.44.5"))
                .Should().BeEquivalentTo(
                    new GeoLocation
                        {
                            Country = "United States",
                            City = "Washington",
                            Point = new Point
                                {
                                    lat = 38.893299999999996,
                                    lon = -77.0146,
                                },
                        },
                    options => options.CompareWithPrecision(precision));
        }

        [Test]
        public void TestLocalAddress()
        {
            var actual = m_resolver.ResolveAddress(IPAddress.Parse("192.168.41.12"));
            actual.Should().BeEquivalentTo((GeoLocation)null);
        }
    }
}