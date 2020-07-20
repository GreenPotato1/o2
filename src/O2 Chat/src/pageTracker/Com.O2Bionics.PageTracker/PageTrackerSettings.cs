using System;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.PageTracker
{
    [SettingsRoot("pageTracker")]
    public class PageTrackerSettings
    {
        [Required]
        [NotWhitespace]
        public string Database { get; [UsedImplicitly] set; }

        [Required]
        public EsConnectionSettings ElasticConnection { get; [UsedImplicitly] set; }

        [Required]
        public EsIndexSettings PageVisitIndex { get; [UsedImplicitly] set; }

        [Required]
        public EsIndexSettings IdStorageIndex { get; [UsedImplicitly] set; }

        [Required]
        [IntRange(1)]
        [Default(1000)]
        public int IdStorageBlockSize { get; [UsedImplicitly] set; }

        [Default(false)]
        public bool LogSqlQuery { get; [UsedImplicitly] set; }

        [Required]
        [NotWhitespace]
        public string MaxMindGeoIpDatabasePath { get; [UsedImplicitly] set; }

        [Required]
        public Uri WidgetUrl { get; [UsedImplicitly] set; }

        [Required]
        public Uri WorkspaceUrl { get; [UsedImplicitly] set; }

        [Required]
        [SettingsRoot("featureServiceClient")]
        public FeatureServiceClientSettings FeatureServiceClient { get; [UsedImplicitly] set; }

        [Required]
        [SettingsRoot("errorTracker")]
        public ErrorTrackerSettings ErrorTracker { get; [UsedImplicitly] set; }

        [IntRange(1)]
        [Default(100)]
        public int AddBufferSize { get; [UsedImplicitly] set; }

        [Default("0:0:5")]
        public TimeSpan AddBufferFlushTimeout { get; [UsedImplicitly] set; }
    }
}