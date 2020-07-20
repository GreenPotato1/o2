using System.Diagnostics;
using System.Net;
using Com.O2Bionics.PageTracker.Tests.Settings;
using Com.O2Bionics.PageTracker.Utilities;
using log4net;
using NUnit.Framework;

namespace Com.O2Bionics.PageTracker.Tests.Performance
{
    [TestFixture]
    [Explicit]
    public class MaxMindGeoIpResolverTests
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(MaxMindGeoIpResolverTests));

        private const int Repeat = 1000000;

        [Test]
        public void TestLookup([Values(0, 1, 2)] int iteration)
        {
            using (var resolver = new MaxMindLocalGeoIpAddressResolver(new TestPageTrackerSettings()))
            {
                var nullCount = 0;
                var sw = Stopwatch.StartNew();
                for (var i = 0; i < Repeat; i++)
                {
                    var bytes = new[]
                        {
                            (byte)TestContext.CurrentContext.Random.Next(1, 255),
                            (byte)TestContext.CurrentContext.Random.Next(1, 255),
                            (byte)TestContext.CurrentContext.Random.Next(1, 255),
                            (byte)TestContext.CurrentContext.Random.Next(1, 255),
                        };
                    var ip = new IPAddress(bytes);

                    var gl = resolver.ResolveAddress(ip);
                    if (gl is null)
                    {
                        // m_log.InfoFormat("{0}", ip);
                        nullCount++;
                    }
                }

                sw.Stop();
                m_log.InfoFormat(
                    "{0} iterations, nulls: {3}, {1:0.000}ms. {2:0.000}rps",
                    Repeat,
                    sw.ElapsedMilliseconds,
                    (double)Repeat * 1000 / sw.ElapsedMilliseconds,
                    nullCount);
            }
        }
    }
}