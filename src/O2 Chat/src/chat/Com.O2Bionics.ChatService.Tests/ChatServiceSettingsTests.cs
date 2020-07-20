using System;
using System.Collections.Generic;
using Com.O2Bionics.AuditTrail.Client.Settings;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.MailerService.Client.Settings;
using Com.O2Bionics.Utils.JsonSettings;
using FluentAssertions;
using NUnit.Framework;

namespace Com.O2Bionics.ChatService.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public sealed class ChatServiceSettingsTests
    {
        [Test]
        public void TestParse()
        {
            const string json = @"{
                    chatService: {
                        wcfBindPort: 8523,
                        database: 'User ID=abakhmet_chat;Password=1;Data Source=//db1.o2bionics.com:1521/ora11;',
                        logSqlQuery: true,
                        cache: {
                            visitor: 1000,
                            session: 1000,
                        },
                        workspaceUrl: 'https://app.chat-parse-test.o2bionics.com',
                    },
                    featureServiceClient: {
                        productCode: 'chat',
                        urls: ['http://fs.chat-parse-test.o2bionics.com/'],
                        timeout: '0:0:5',
                        localCacheTimeToLiveSeconds: 2,
                    },
                    mailerServiceClient: {
                        urls: ['http://mail-parse-test.o2bionics.com/'],
                    },
                    errorTracker: {
                        elasticConnection: { uris: ['http://127.0.0.1:9200'] },
                        'index': {
                            'name': 'someindex',
                            'settings': {
                                'index.number_of_shards': '50',
                                'index.translog.durability': 'async',
                                'index.translog.sync_interval': '52s',
                                'index.refresh_interval': '120s',
                            },
                        },
                        emergencyLogDirectory: 'C:\\O2Bionics\\O2Chat\\Logs'
                    },
                    auditTrailClient: {
                        urls: [ 'http://ats.chat-parse-test.o2bionics.com/']
                    },
                    widgetLoadLimiter: {
                        countersDbUpdateDelta: 100,
                        countersDbUpdateMinimumIntervalSeconds: 20,
                    },
                }";

            var s = new JsonSettingsReader().ReadFromString<ChatServiceSettings>(json);
            s.Should().BeEquivalentTo(
                new ChatServiceSettings
                    {
                        WcfBindPort = 8523,
                        Database = "User ID=abakhmet_chat;Password=1;Data Source=//db1.o2bionics.com:1521/ora11;",
                        LogSqlQuery = true,
                        Cache = new ChatServiceCacheSettings
                            {
                                Visitor = 1000,
                                Session = 1000,
                            },
                        WorkspaceUrl = new Uri("https://app.chat-parse-test.o2bionics.com"),
                        MailerServiceClient = new MailerServiceClientSettings
                            {
                                Urls = new List<Uri> { new Uri("http://mail-parse-test.o2bionics.com/") },
                            },
                        FeatureServiceClient = new FeatureServiceClientSettings
                            {
                                ProductCode = "chat",
                                Urls = new List<Uri> { new Uri("http://fs.chat-parse-test.o2bionics.com/") },
                                Timeout = TimeSpan.FromSeconds(5),
                                LocalCacheTimeToLiveSeconds = 2
                            },
                        ErrorTracker = new ErrorTrackerSettings
                            {
                                ElasticConnection = new EsConnectionSettings(new Uri("http://127.0.0.1:9200")),
                                Index = new EsIndexSettings
                                {
                                    Name = "someindex",
                                    Settings = new Dictionary<string, string>
                                        {
                                            {"index.number_of_shards", "50"},
                                            {"index.translog.durability", "async"},
                                            {"index.translog.sync_interval", "52s"},
                                            {"index.refresh_interval", "120s"},
                                        },
                                },
                                EmergencyLogDirectory = "C:\\O2Bionics\\O2Chat\\Logs"
                            },
                        AuditTrailClient = new AuditTrailClientSettings
                            {
                                Urls = new List<Uri> { new Uri("http://ats.chat-parse-test.o2bionics.com/") }
                            },
                        WidgetLoadLimiterSettings = new WidgetLoadLimiterSettings
                            {
                                CountersDbUpdateDelta = 100,
                                CountersDbUpdateMinimumIntervalSeconds = 20,
                            },
                    });
        }
    }
}