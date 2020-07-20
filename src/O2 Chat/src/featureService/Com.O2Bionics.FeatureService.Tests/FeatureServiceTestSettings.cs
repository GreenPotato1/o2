using System;
using System.Collections.Generic;
using Com.O2Bionics.FeatureService.Impl;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.FeatureService.Tests
{
    public class FeatureServiceTestSettings : FeatureServiceSettings
    {
        public FeatureServiceTestSettings()
        {
            SelfHostWebBindUri = "http://*:8081";

            Databases = new Dictionary<string, string>
                    { { DatabaseHelper.TestProductCode, new JsonSettingsReader().ReadFromFile<TestSettings>().FeatureServiceDatabase } };

            LogSqlQuery = true;
            LogProcessing = true;
            TimeToLive = TimeSpan.FromSeconds(10);

            Cache = new FeatureServiceCacheSettings
                {
                    MemoryLimitMegabytes = 0,
                    PhysicalMemoryLimitPercentage = 0,
                    MemoryPollingInterval = TimeSpan.FromMinutes(1),
                };
        }
    }
}