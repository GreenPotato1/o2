using System;
using Com.O2Bionics.AuditTrail.Client.Settings;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.MailerService.Client.Settings;
using Com.O2Bionics.Tests.Common;
using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.ChatService.Tests
{
    // tests should not use machine settings besides TestSettings
    public class TestChatServiceSettings : ChatServiceSettings
    {
        public const int DefaultChatServicePort = 8528;

        public TestChatServiceSettings()
        {
            Database = new JsonSettingsReader().ReadFromFile<TestSettings>().ChatServiceDatabase;

            WcfBindPort = DefaultChatServicePort;
            LogSqlQuery = true;

            Cache = new ChatServiceCacheSettings
                {
                    Visitor = 10,
                    Session = 10,
                };
            MailerServiceClient = new MailerServiceClientSettings
                {
                    Urls = new[] { new Uri("http://mailer.bad-domain"), },
                };
            FeatureServiceClient = new FeatureServiceClientSettings
                {
                    Urls = new[] { new Uri("http://fs.bad-domain"), },
                    Timeout = TimeSpan.FromSeconds(1),
                    ProductCode = "chat",
                    LocalCacheTimeToLiveSeconds = 0
                };
            AuditTrailClient = new AuditTrailClientSettings
                {
                    Urls = new[] { new Uri("http://atc.bad-domain"), },
                };
            WidgetLoadLimiterSettings = new WidgetLoadLimiterSettings
                {
                    CountersDbUpdateMinimumIntervalSeconds = 100,
                    CountersDbUpdateDelta = 300
                };
        }
    }
}