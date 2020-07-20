using System;
using System.Linq;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;
using NUnit.Framework;

namespace Com.O2Bionics.PageTracker.Tests.Settings
{
    public sealed class TestPageTrackerSettings : PageTrackerSettings
    {
        public TestPageTrackerSettings()
        {
            var testSettings = new JsonSettingsReader().ReadFromFile<TestSettings>();
            var pageTrackerSettings = new JsonSettingsReader().ReadFromFile<PageTrackerSettings>();
            Assert.IsNotNull(pageTrackerSettings, nameof(pageTrackerSettings));

            Database = testSettings.PageTrackerServiceDatabase;
            LogSqlQuery = true;

            MaxMindGeoIpDatabasePath = pageTrackerSettings.MaxMindGeoIpDatabasePath;

            WidgetUrl = new Uri(pageTrackerSettings.WidgetUrl.AbsoluteUri);
            WorkspaceUrl = new Uri(pageTrackerSettings.WorkspaceUrl.AbsoluteUri);
            FeatureServiceClient = new FeatureServiceClientSettings
                {
                    Urls = pageTrackerSettings.FeatureServiceClient.Urls.ToList(),
                    Timeout = TimeSpan.FromSeconds(60),
                    ProductCode = "chat",
                };

            ElasticConnection = pageTrackerSettings.ElasticConnection;
            PageVisitIndex = new EsIndexSettings
                {
                    Name = pageTrackerSettings.PageVisitIndex.Name + "_test",
                    Settings = pageTrackerSettings.PageVisitIndex.Settings,
                };
            IdStorageIndex = new EsIndexSettings
                {
                    Name = pageTrackerSettings.IdStorageIndex.Name + "_test",
                    Settings = pageTrackerSettings.IdStorageIndex.Settings,
                };

            IdStorageBlockSize = 20;
            AddBufferFlushTimeout = pageTrackerSettings.AddBufferFlushTimeout;
            AddBufferSize = pageTrackerSettings.AddBufferSize;
        }
    }
}