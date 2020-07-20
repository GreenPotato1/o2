using System;
using System.Collections.Generic;
using Com.O2Bionics.FeatureService.Impl;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;
using NUnit.Framework;

namespace Com.O2Bionics.FeatureService.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class FeatureServiceSettingsTests
    {
        [Test]
        public void TestParse()
        {
            const string text = @"{
                    featureService: {
                        selfHostWebBindUri: 'test://asd:20/',
                        databases: {
                            'chat': 'database value1',
                            'test': 'database value2',
                        },
                        logSqlQuery: true,
                        logProcessing: true,
                        timeToLive: '1:2:4',
                        cache: {
                            memoryLimitMegabytes: 10,
                            physicalMemoryLimitPercentage: 20,
                            memoryPollingInterval: '3:5:6',
                        },
                    },
                    errorTracker: {
                        elasticConnection: { uris: ['http://127.0.0.1:9200'] },
                        'index': {
                            'name': 'someindex',
                            'settings': {
                                'index.number_of_shards': '70',
                                'index.translog.durability': 'async',
                                'index.translog.sync_interval': '52s',
                                'index.refresh_interval': '120s',
                            },
                        },
                        emergencyLogDirectory: 'C:\\O2Bionics\\O2Chat\\Logs'
                    }
                }";

            var s = new JsonSettingsReader().ReadFromString<FeatureServiceSettings>(text);
            s.Should().BeEquivalentTo(
                new FeatureServiceSettings
                    {
                        SelfHostWebBindUri = "test://asd:20/",
                        Databases = new Dictionary<string, string> { { "chat", "database value1" }, { "test", "database value2" } },
                        LogSqlQuery = bool.Parse("true"),
                        LogProcessing = bool.Parse("true"),
                        TimeToLive = TimeSpan.Parse("1:2:4"),
                        Cache = new FeatureServiceCacheSettings
                            {
                                MemoryLimitMegabytes = int.Parse("10"),
                                PhysicalMemoryLimitPercentage = int.Parse("20"),
                                MemoryPollingInterval = TimeSpan.Parse("3:5:6"),
                            },
                        ErrorTracker = new ErrorTrackerSettings
                            {
                                ElasticConnection = new EsConnectionSettings(new Uri("http://127.0.0.1:9200")),
                                Index = new EsIndexSettings
                                    {
                                        Name = "someindex",
                                        Settings = new Dictionary<string, string>
                                            {
                                                {"index.number_of_shards", "70"},
                                                {"index.translog.durability", "async"},
                                                {"index.translog.sync_interval", "52s"},
                                                {"index.refresh_interval", "120s"},
                                            },
                                    },
                                EmergencyLogDirectory = "C:\\O2Bionics\\O2Chat\\Logs"
                            }
                    });
        }
    }
}