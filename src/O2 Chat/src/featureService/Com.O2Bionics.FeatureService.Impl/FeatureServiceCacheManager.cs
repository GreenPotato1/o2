using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Caching;

namespace Com.O2Bionics.FeatureService.Impl
{
    public class FeatureServiceCacheManager : IFeatureServiceCacheManager
    {
        private readonly FeatureServiceSettings m_settings;

        private MemoryCache m_cache;

        public FeatureServiceCacheManager(FeatureServiceSettings settings)
        {
            m_settings = settings;
            m_cache = CreateCache();
        }

        private MemoryCache CreateCache()
        {
            var config = new NameValueCollection
                {
                        { "CacheMemoryLimitMegabytes", m_settings.Cache.MemoryLimitMegabytes.ToString("D") },
                        { "PhysicalMemoryLimitPercentage", m_settings.Cache.PhysicalMemoryLimitPercentage.ToString("D") },
                        { "PollingInterval", m_settings.Cache.MemoryPollingInterval.ToString("c") },
                };
            return new MemoryCache("FeatureServiceCache", config);
        }

        public string GetKey(string productCode, int userId, string featureCode)
        {
            return "fv-" + productCode + "-" + userId.ToString(CultureInfo.InvariantCulture) + "-" + featureCode;
        }

        public string GetValue(string cacheKey)
        {
            return (string)m_cache[cacheKey];
        }

        public void SetValue(string cacheKey, string value, int? timeToLiveSeconds)
        {
            var actualTimeToLiveSeconds = timeToLiveSeconds ?? m_settings.TimeToLive.TotalSeconds;
            if (actualTimeToLiveSeconds > 0)
            {
                var policy = new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(actualTimeToLiveSeconds),
                    };
                m_cache.Add(cacheKey, value, policy);
            }
        }

        public void ResetCache()
        {
            var cache = m_cache;
            m_cache = CreateCache();
            cache.Dispose();
        }

        public void Dispose()
        {
            m_cache.Dispose();
        }
    }
}