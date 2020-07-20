using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using Com.O2Bionics.Utils.Properties;
using JetBrains.Annotations;
using log4net;
using Newtonsoft.Json;

namespace Com.O2Bionics.FeatureService.Client
{
    public sealed class FeatureServiceClient : IFeatureServiceClient
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(FeatureServiceClient));

        [CanBeNull] private readonly ObjectCache m_cache;
        [NotNull] private readonly INowProvider m_nowProvider;
        [NotNull] private readonly HttpClientWrap m_httpClient;

        private readonly string m_productCode;
        private readonly bool? m_ignoreCache;
        private readonly int m_localCacheTimeToLiveSeconds;
        private readonly bool? m_logProcessing;
        private readonly bool? m_logSqlQuery;
        private readonly int? m_ttl;
        private readonly Uri[] m_uris;

        public FeatureServiceClient(
            [NotNull] FeatureServiceClientSettings settings,
            [CanBeNull] ObjectCache cache,
            [NotNull] INowProvider nowProvider,
            string overrideProductCode = null,
            bool? ignoreCache = null,
            int? ttl = null,
            bool? logProcessing = null,
            bool? logSqlQuery = null)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.ProductCode))
                throw new ArgumentException("The FeatureService Settings ProductCode can't be null or whitespace.");

            if (settings.LocalCacheTimeToLiveSeconds > 0 && cache == null)
                throw new ArgumentNullException(nameof(cache));
            m_cache = cache;

            m_nowProvider = nowProvider ?? throw new ArgumentNullException(nameof(nowProvider));

            var ignoreCacheValue = ignoreCache.HasValue && ignoreCache.Value;
            if (!ignoreCacheValue)
                m_localCacheTimeToLiveSeconds = settings.LocalCacheTimeToLiveSeconds;

            m_ignoreCache = ignoreCache;
            m_ttl = ttl;
            m_logProcessing = logProcessing;
            m_logSqlQuery = logSqlQuery;
            m_uris = BuildUris(settings.Urls);

            if (overrideProductCode != null && string.IsNullOrWhiteSpace(overrideProductCode))
                throw new ArgumentException("The overrideProductCode can't be whitespace");
            m_productCode = overrideProductCode ?? settings.ProductCode;

            var timeout = settings.Timeout == TimeSpan.Zero ? TimeSpan.FromMilliseconds(-1.0) : settings.Timeout;
            m_httpClient = new HttpClientWrap();
            m_httpClient.HttpClient.Timeout = timeout;

            if (m_log.IsDebugEnabled)
                m_log.DebugFormat(
                    "created: pc='{0}' uri=[{1}], timeout={2}ms.",
                    m_productCode,
                    string.Join(", ", m_uris.Select(x => x.ToString())),
                    timeout);
        }

        public void Dispose()
        {
            m_httpClient.Dispose();
        }

        public async Task<Dictionary<string, string>> GetValue(uint userId, List<string> featureCodes)
        {
            var result = await GetValue(userId, featureCodes, m_httpClient).ConfigureAwait(false);
            return result;
        }

        internal async Task<Dictionary<string, string>> GetValue(uint userId, List<string> featureCodes, [NotNull] IHttpClientWrap httpClient)
        {
            if (userId <= 0)
                throw new ArgumentException(string.Format(Resources.ArgumentMustBePositive2, nameof(userId), userId));

            if (featureCodes == null)
                throw new ArgumentNullException(nameof(featureCodes));
            if (0 == featureCodes.Count)
                throw new ArgumentException("Can't be empty", nameof(featureCodes));
            if (featureCodes.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Can't contain null or whitespace", nameof(featureCodes));

            if (m_log.IsDebugEnabled)
                m_log.DebugFormat(
                    "GetValue: pc={0} uid={1} fc=[{2}] ic={3} ttl={4} logp={5} logq={6}",
                    m_productCode,
                    userId,
                    string.Join(",", featureCodes),
                    m_ignoreCache,
                    m_ttl,
                    m_logProcessing,
                    m_logSqlQuery);

            string prefix = null;

            List<string> notCachedFeatureCodes;
            Dictionary<string, string> cached = null;
            if (0 < m_localCacheTimeToLiveSeconds)
            {
                prefix = Prefix(m_productCode, userId);
                GetFromCache(prefix, featureCodes, out notCachedFeatureCodes, out cached);

                if (null != cached && m_log.IsDebugEnabled)
                    m_log.DebugFormat(
                        "GetValue, cached response [{0}]",
                        string.Join(",", cached.Select(x => $"{x.Key}: {x.Value}")));
                if (notCachedFeatureCodes == null)
                    return cached;
            }
            else
                notCachedFeatureCodes = featureCodes;

            Debug.Assert(null != notCachedFeatureCodes && 0 < notCachedFeatureCodes.Count);
            var parameters = GetParameters(userId, notCachedFeatureCodes, m_productCode);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < m_uris.Length; i++)
            {
                var uri = AddQuery(m_uris[i], parameters);
                var result = await CallGet(httpClient, uri).ConfigureAwait(false);
                if (result == null)
                    continue;
                if (m_log.IsDebugEnabled)
                    m_log.DebugFormat(
                        "GetValue {0} response [{1}]",
                        uri,
                        string.Join(",", result.Select(x => $"{x.Key}: {x.Value}")));

                if (result.TryGetValue("error", out var errorType))
                    throw FeatureServiceCallResult.CreateException(errorType, result["errorMessage"]);

                if (0 < m_localCacheTimeToLiveSeconds)
                    PutToCache(prefix, notCachedFeatureCodes, result);

                if (null != cached)
                    foreach (var p in cached)
                        result[p.Key] = p.Value;

                return result;
            }

            throw new Exception("GetValue failed");
        }

        private NameValueCollection GetParameters(uint userId, List<string> featureCodes, string productCode)
        {
            var parameters = new NameValueCollection
                {
                    { "pc", productCode },
                    { "uid", userId.ToString() },
                    { "fc", string.Join(",", featureCodes) }
                };
            if (m_ignoreCache.HasValue)
                parameters.Add("ic", m_ignoreCache.Value ? FeatureValues.True : FeatureValues.False);
            if (m_ttl.HasValue)
                parameters.Add("ttl", m_ttl.Value.ToString());
            if (m_logProcessing.HasValue)
                parameters.Add("logp", m_logProcessing.Value ? FeatureValues.True : FeatureValues.False);
            if (m_logSqlQuery.HasValue)
                parameters.Add("logq", m_logSqlQuery.Value ? FeatureValues.True : FeatureValues.False);
            return parameters;
        }

        [Pure]
        private static Uri[] BuildUris(IReadOnlyCollection<Uri> uris)
        {
            if (uris == null)
                throw new ArgumentException("settings.Urls can't be null");
            if (uris.Count == 0)
                throw new ArgumentException("settings.Urls can't be empty");
            if (uris.Any(x => x == null))
                throw new ArgumentException("settings.Urls can't contain null");

            return uris.Select(x => new Uri(x, "/get")).ToArray();
        }

        private static async Task<Dictionary<string, string>> CallGet(IHttpClientWrap httpClient, Uri uri)
        {
            m_log.DebugFormat("sending request: {0}", uri);

            try
            {
                using (var response = await httpClient.GetAsync(uri).ConfigureAwait(false))
                {
                    var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (response.IsSuccessStatusCode) return JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

                    m_log.Error($"Query to {uri} failed with code {response.StatusCode:G}.");
                }
            }
            catch (Exception e)
            {
                m_log.Error($"Query to {uri} failed with exception.", e);
            }

            return null;
        }

        [Pure]
        private static Uri AddQuery(Uri uri, NameValueCollection values)
        {
            var ub = new UriBuilder(uri);

            var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

            foreach (var key in values.AllKeys)
                httpValueCollection.Add(key, values[key]);

            ub.Query = httpValueCollection.ToString();

            return ub.Uri;
        }

        [Pure]
        private static string Prefix([NotNull] string productCode, uint userId)
        {
            return "fsc-" + productCode + "-" + userId + "-";
        }

        [Pure]
        private static string Key([NotNull] string prefix, [NotNull] string featureCode)
        {
            return prefix + featureCode;
        }

        private static readonly object m_nullValue = new object();

        private void GetFromCache(
            [NotNull] string prefix,
            [NotNull] List<string> featureCodes,
            [CanBeNull] out List<string> notCachedFeatureCodes,
            [CanBeNull] out Dictionary<string, string> cached)
        {
            Debug.Assert(m_cache != null);

            notCachedFeatureCodes = null;
            cached = null;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < featureCodes.Count; ++i)
            {
                var key = Key(prefix, featureCodes[i]);
                var cacheValue = m_cache[key];
                if (cacheValue == null)
                {
                    notCachedFeatureCodes = notCachedFeatureCodes ?? new List<string>();
                    notCachedFeatureCodes.Add(featureCodes[i]);
                }
                else
                {
                    var value = m_nullValue == cacheValue ? null : (string)cacheValue;
                    cached = cached ?? new Dictionary<string, string>();
                    cached[featureCodes[i]] = value;
                }
            }
        }

        private void PutToCache(string prefix, [NotNull] List<string> featureCodes, [NotNull] Dictionary<string, string> values)
        {
            Debug.Assert(m_cache != null);
            Debug.Assert(0 < featureCodes.Count);
            Debug.Assert(values.Count <= featureCodes.Count);

            var now = m_nowProvider.UtcNow;
            var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = now.AddSeconds(m_localCacheTimeToLiveSeconds),
                };

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < featureCodes.Count; i++)
            {
                if (values.TryGetValue(featureCodes[i], out var value))
                {
                    var key = Key(prefix, featureCodes[i]);
                    m_cache.Add(key, value ?? m_nullValue, policy);
                }
            }
        }
    }
}