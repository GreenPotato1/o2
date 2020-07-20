using System;
using System.Collections.Generic;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;
using NUnit.Framework;

namespace Com.O2Bionics.PageTracker.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public sealed class PageTrackerSettingsTests
    {
        [Test]
        public void TestParse()
        {
            const string text = @"{
                    pageTracker: {
                        database: 'database value',
                        logSqlQuery: true,
                        elasticConnection : { uris: ['http://[es1,es2].elastic.parse-test.local'] },
                        pageVisitIndex: {
                            name: 'abakhmet_track',
                            settings: {
                                'index.number_of_shards': '5',
                                'index.translog.durability': 'async',
                                'index.translog.sync_interval': '5s',
                            }
                        },
                        idStorageIndex: {
                            name: 'abakhmet_track_ids',
                        },
                        idStorageBlockSize : 100,
                        maxMindGeoIpDatabasePath: 'some path',
                        widgetUrl: 'http://site.origin',
                        workspaceUrl: 'https://other.site.origin/',
                        addBufferFlushTimeout: '0:0:42',
                        addBufferSize: 102,
                    },
                    featureServiceClient: {
                        urls: [ 'http://asd.asd/', 'https://aaa.aaa.bbb' ],
                        timeout: '1:2:4',
                        productCode: 'code1',
                        localCacheTimeToLiveSeconds: 7,
                    },
                    errorTracker: {
                        elasticConnection: { uris: ['http://127.0.0.1:9200'] },
                        'index': {
                            'name': 'someindex',
                            'settings': {
                                'index.number_of_shards': '20',
                                'index.translog.durability': 'async',
                                'index.translog.sync_interval': '52s',
                                'index.refresh_interval': '120s',
                            },
                        },
                        emergencyLogDirectory: 'C:\\O2Bionics\\O2Chat\\Logs'
                    }
                }";

            var s = new JsonSettingsReader().ReadFromString<PageTrackerSettings>(text);
            s.Should().BeEquivalentTo(
                new PageTrackerSettings
                    {
                        Database = "database value",
                        LogSqlQuery = true,
                        ElasticConnection = new EsConnectionSettings(
                            new Uri("http://es1.elastic.parse-test.local"),
                            new Uri("http://es2.elastic.parse-test.local")),
                        PageVisitIndex = new EsIndexSettings
                            {
                                Name = "abakhmet_track",
                                Settings = new Dictionary<string, string>
                                    {
                                        { "index.number_of_shards", "5" },
                                        { "index.translog.durability", "async" },
                                        { "index.translog.sync_interval", "5s" },
                                    }
                            },
                        IdStorageIndex = new EsIndexSettings
                            {
                                Name = "abakhmet_track_ids",
                                Settings = null,
                            },
                        MaxMindGeoIpDatabasePath = "some path",
                        WidgetUrl = new Uri("http://site.origin"),
                        WorkspaceUrl = new Uri("https://other.site.origin/"),
                        FeatureServiceClient = new FeatureServiceClientSettings
                            {
                                Urls = new[] { new Uri("http://asd.asd/"), new Uri("https://aaa.aaa.bbb") },
                                Timeout = TimeSpan.Parse("1:2:4"),
                                ProductCode = "code1",
                                LocalCacheTimeToLiveSeconds = 7
                            },
                        ErrorTracker = new ErrorTrackerSettings
                            {
                                ElasticConnection = new EsConnectionSettings(new Uri("http://127.0.0.1:9200")),
                                Index = new EsIndexSettings
                                    {
                                        Name = "someindex",
                                        Settings = new Dictionary<string, string>
                                            {
                                                {"index.number_of_shards", "20"},
                                                {"index.translog.durability", "async"},
                                                {"index.translog.sync_interval", "52s"},
                                                {"index.refresh_interval", "120s"},
                                            },
                                    },
                                EmergencyLogDirectory = "C:\\O2Bionics\\O2Chat\\Logs"
                            },
                        IdStorageBlockSize = 100,
                        AddBufferFlushTimeout = TimeSpan.FromSeconds(42),
                        AddBufferSize = 102,
                    });
        }

        [Test]
        public void TestParseEmpty()
        {
            Action a = () => new JsonSettingsReader().ReadFromString<PageTrackerSettings>("{ pageTracker: { } }");
            var ex = a.Should().Throw<JsonSettingsErrorsException>().Which;
            Console.WriteLine(ex);
            ex.Errors.Should().BeEquivalentTo(
                "PageTrackerSettings.Database value is required",
                "PageTrackerSettings.ElasticConnection value is required",
                "PageTrackerSettings.PageVisitIndex value is required",
                "PageTrackerSettings.IdStorageIndex value is required",
                "PageTrackerSettings.MaxMindGeoIpDatabasePath value is required",
                "PageTrackerSettings.WidgetUrl value is required",
                "PageTrackerSettings.WorkspaceUrl value is required",
                "PageTrackerSettings.FeatureServiceClient value is required",
                "PageTrackerSettings.FeatureServiceClient uses [SettingsRoot('featureServiceClient')] but the corresponding root level entry was not found",
                "PageTrackerSettings.ErrorTracker value is required",
                "PageTrackerSettings.ErrorTracker uses [SettingsRoot('errorTracker')] but the corresponding root level entry was not found");
        }
    }
}