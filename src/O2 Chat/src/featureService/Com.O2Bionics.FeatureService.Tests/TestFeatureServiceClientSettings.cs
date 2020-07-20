using System;
using System.Collections.Generic;
using Com.O2Bionics.FeatureService.Client;

namespace Com.O2Bionics.FeatureService.Tests
{
    public class TestFeatureServiceClientSettings : FeatureServiceClientSettings
    {
        public TestFeatureServiceClientSettings(
            string featureServiceProductCode,
            IReadOnlyCollection<Uri> featureServiceUrls,
            int localCacheTimeToLiveSeconds = 20,
            // 5 seconds are not enough - tests are flaky
            int timeoutSeconds = 20)
        {
            Urls = featureServiceUrls;
            Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            ProductCode = featureServiceProductCode;
            LocalCacheTimeToLiveSeconds = localCacheTimeToLiveSeconds;
        }
    }
}