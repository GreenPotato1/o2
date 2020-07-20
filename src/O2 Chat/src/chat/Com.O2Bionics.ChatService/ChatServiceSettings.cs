using System;
using Com.O2Bionics.AuditTrail.Client.Settings;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.MailerService.Client.Settings;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.ChatService
{
    [SettingsRoot("chatService")]
    public class ChatServiceSettings
    {
        [Default(8523)]
        [IntRange(1)]
        public int WcfBindPort { get; set; }

        [Required]
        [NotWhitespace]
        public string Database { get; set; }

        [Default(false)]
        public bool LogSqlQuery { get; set; }

        public ChatServiceCacheSettings Cache { get; set; }

        [SettingsRoot(MailerServiceClientSettings.RootName)]
        [Required]
        public MailerServiceClientSettings MailerServiceClient { get; [UsedImplicitly] set; }

        [Required]
        public Uri WorkspaceUrl { get; set; }

        [SettingsRoot("featureServiceClient")]
        [Required]
        public FeatureServiceClientSettings FeatureServiceClient { get; set; }

        [Required]
        [SettingsRoot("errorTracker")]
        public ErrorTrackerSettings ErrorTracker { get; set; }

        [Required]
        [SettingsRoot(AuditTrailClientSettings.RootName)]
        public AuditTrailClientSettings AuditTrailClient { get; [UsedImplicitly] set; }

        [Required]
        [SettingsRoot("widgetLoadLimiter")]
        public WidgetLoadLimiterSettings WidgetLoadLimiterSettings { get; [UsedImplicitly] set; }
    }

    [SettingsClass]
    public class ChatServiceCacheSettings
    {
        [Default(10000)]
        [IntRange(0)]
        public int Visitor { get; set; }

        [Default(1000)]
        [IntRange(0)]
        public int Session { get; set; }
    }

    [SettingsClass]
    public sealed class WidgetLoadLimiterSettings
    {
        /// <summary>
        ///     After these many page loads per customer, the data will be saved
        ///     to the database unless there has been less than 
        ///     <seealso cref="CountersDbUpdateMinimumIntervalSeconds"/> after
        ///     the last save.
        /// </summary>
        [Required]
        [IntRange(1)]
        public int CountersDbUpdateDelta { get; [UsedImplicitly] set; }

        [Required]
        [IntRange(1, 86400)]
        public int CountersDbUpdateMinimumIntervalSeconds { get; [UsedImplicitly] set; }
    }
}