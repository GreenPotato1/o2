using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.PageTracker.Storage;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Elasticsearch.Net;
using FluentAssertions;
using Jil;
using Nest;
using NUnit.Framework;
using GeoLocation = Com.O2Bionics.PageTracker.Contract.GeoLocation;

namespace Com.O2Bionics.PageTracker.Tests.Performance
{
    [TestFixture]
    [Explicit]
    public class ElasticAccessTests : PageTrackerTestsBase
    {
        private const int ThreadsCount = 5;
        private const int Iterations = 1000;

        [SetUp]
        public void SetUp()
        {
            PageTrackerIndexHelper.DeleteIndices(Settings);
            PageTrackerIndexHelper.CreateIndices(Settings);
        }

        [Test]
        public void TestNest([Values(1, 2, 3)] int repeat)
        {
            var client = new EsClient(Settings.ElasticConnection).Client;

            void IndexOneDocument(PageView doc)
            {
                var result = client.Index(doc, x => x.Index(Settings.PageVisitIndex.Name));
                result.Result.Should().Be(Result.Created);
            }

            RunTest(nameof(TestNest), IndexOneDocument, Iterations);
        }

        [Test]
        public void TestLowLevelClient([Values(1, 2, 3)] int repeat)
        {
            var client = new EsClient(Settings.ElasticConnection).Client.LowLevel;

            void IndexOneDocument(PageView doc)
            {
                var json = JSON.Serialize(doc, JsonSerializerBuilder.SkipNullJilOptions);
                var result = client.Index<StringResponse>(
                    Settings.PageVisitIndex.Name,
                    FieldConstants.PreferredTypeName,
                    PostData.String(json));
                result.Success.Should().BeTrue();
            }

            RunTest(nameof(TestLowLevelClient), IndexOneDocument, Iterations);
        }

        [Test]
        public void TestHttpClient([Values(1, 2, 3)] int repeat)
        {
            var httpClient = new HttpClient();
            var indexUri = new Uri(Settings.ElasticConnection.Uris.First(), Settings.PageVisitIndex.Name + "/doc");

            void IndexOneDocument(PageView doc)
            {
                var json = JSON.Serialize(doc, JsonSerializerBuilder.SkipNullJilOptions);
                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var result = httpClient.PostAsync(indexUri, content).WaitAndUnwrapException())
                {
                    result.ReasonPhrase.Should().Be("Created");
                }
            }

            RunTest(nameof(TestHttpClient), IndexOneDocument, Iterations);
        }

        private long m_pageVisitCounter;

        public void RunTest(string testName, Action<PageView> indexOneDocument, int iterations)
        {
            m_pageVisitCounter = 0;

            var threads = new BackgroundThreads(
                ThreadsCount,
                _ =>
                    {
                        for (var i = 0; i < iterations; i++)
                        {
                            var doc = CreateDocument(i);
                            indexOneDocument(doc);
                        }
                    });

            LogHelper.WithLogLevel(
                log4net.Core.Level.Info,
                () => threads.Measure(nameof(ElasticAccessTests) + "." + testName, "index", iterations));
        }

        private PageView CreateDocument(int index)
        {
            return new PageView
                {
                    Id = ((ulong)Interlocked.Increment(ref m_pageVisitCounter)).ToString(),
                    Timestamp = DateTime.UtcNow,
                    VisitorId = EsUnsignedHelper.ToEs(1),
                    VisitorExternalId = "test " + index,
                    CustomerId = 1,
                    UriInfo = new UriInfo
                        {
                            Url = "http://test.com/test",
                            Host = "test.com",
                            Scheme = "http://",
                            Path = "/test",
                        },
                    UserAgent = new UserAgentInfo
                        {
                            UserAgentString = "some user agent string",
                            UserAgent = "chrome",
                            Os = "mazdai",
                            Device = "coffeemaker",
                        },
                    TimeZone = new TimeZoneDescription(0, "test"),
                    CustomText = "custom text",
                    Location = new GeoLocation(),
                    IpAddress = "127.0.0.1",
                };
        }
    }
}