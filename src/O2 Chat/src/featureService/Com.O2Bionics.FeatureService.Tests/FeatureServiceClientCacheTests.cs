using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;
using NUnit.Framework;
using NSubstitute;
using NSubstitute.Core;

namespace Com.O2Bionics.FeatureService.Tests
{
    [TestFixture]
    [Parallelizable]
    public sealed class FeatureServiceClientCacheTests
    {
        private const int CacheTtlSeconds = 1;

        private const string ProductCode = "Apple";
        private const string FeatureCode = "Banana";
        private const string Url = "https://localhost:123/";

        private const string UrlGet =
            Url + "get?pc=" + ProductCode + "&uid=" + TestConstants.CustomerIdString + "&fc=" + FeatureCode;

        private static List<string> FeatureCodes => new List<string> { FeatureCode };

        private readonly Dictionary<string, string> m_expected = new Dictionary<string, string>();
        private readonly DefaultNowProvider m_nowProvider = new DefaultNowProvider();

        private IHttpClientWrap m_httpClient;
        private MemoryCache m_cache;

        private string m_response;

        [SetUp]
        public void SetUp()
        {
            m_expected.Clear();

            m_httpClient = Substitute.For<IHttpClientWrap>();
            Task<HttpResponseMessage> GetMock(CallInfo s)
            {
                Assert.That(s.Arg<Uri>(), Is.EqualTo(new Uri(UrlGet)));

                var message = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(m_response) };
                return Task.FromResult(message);
            }
            m_httpClient.GetAsync(Arg.Any<Uri>()).Returns(GetMock);
            
            m_cache = new MemoryCache(nameof(FeatureServiceClientCacheTests));
        }

        [TearDown]
        public void TearDown()
        {
            m_cache.Dispose();
            m_httpClient.Dispose();
        }

        [Test]
        public async Task TestGetValue(
            [Values(null, "", "Cabbage")] [CanBeNull]
            string featureValue,
            [Values(false, true)] bool useCache)
        {
            m_expected[FeatureCode] = featureValue;
            m_response = m_expected.JsonStringify2();

            var settings = new FeatureServiceClientSettings
                {
                    Urls = new List<Uri> { new Uri(Url) },
                    ProductCode = ProductCode,
                    Timeout = TimeSpan.FromTicks(1),
                    LocalCacheTimeToLiveSeconds = useCache ? CacheTtlSeconds : 0,
                };
            using (var client = new FeatureServiceClient(settings, m_cache, m_nowProvider))
            {
                await m_httpClient.Received(0).GetAsync(Arg.Any<Uri>());

                await CallGetValue(client, 1);
                await CallGetValue(client, useCache ? 1 : 2);

                if (useCache)
                {
                    const int ensureCacheExpiration = 1;
                    Thread.Sleep(1000 * (CacheTtlSeconds + ensureCacheExpiration));

                    await CallGetValue(client, 2);
                }
            }
        }

        private async Task CallGetValue(FeatureServiceClient client, int expectedHttpGetCallCount)
        {
            var values = await client.GetValue(TestConstants.CustomerId, FeatureCodes, m_httpClient);
            Assert.That(values, Is.EqualTo(m_expected));
            await m_httpClient.Received(expectedHttpGetCallCount).GetAsync(Arg.Any<Uri>());
        }
    }
}