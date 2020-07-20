using System;
using System.Collections.Generic;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils.JsonSettings;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Com.O2Bionics.FeatureService.Impl
{
    [SettingsRoot("featureService")]
    public class FeatureServiceSettings
    {
        [Default("http://*:8080")]
        [NotWhitespace]
        public string SelfHostWebBindUri { get; set; }

        [Required]
        [NotEmpty]
        public IReadOnlyDictionary<string, string> Databases { get; set; }

        [Default(false)]
        public bool LogSqlQuery { get; set; }

        [Default(false)]
        public bool LogProcessing { get; set; }

        [Default("0:1:0")]
        public TimeSpan TimeToLive { get; set; }

        public FeatureServiceCacheSettings Cache { get; set; }

        [Required]
        [SettingsRoot("errorTracker")]
        public ErrorTrackerSettings ErrorTracker { get; set; }
    }

    [SettingsClass]
    public class FeatureServiceCacheSettings
    {
        [Default(0)]
        public int MemoryLimitMegabytes { get; set; }

        [Default(0)]
        public int PhysicalMemoryLimitPercentage { get; set; }

        [Default("0:1:0")]
        public TimeSpan MemoryPollingInterval { get; set; }
    }
}