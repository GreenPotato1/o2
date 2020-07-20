using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.Tests.Common
{
    [SettingsRoot("test")]
    public class TestSettings
    {
        [Default("")]
        public string ChatServiceDatabase { get; [UsedImplicitly] set; }

        [Default("")]
        public string FeatureServiceDatabase { get; [UsedImplicitly] set; }

        [Default("")]
        public string PageTrackerServiceDatabase { get; [UsedImplicitly] set; }

        [SettingsRoot("errorTracker")]
        [Required]
        public ErrorTrackerSettings ErrorTracker { get; [UsedImplicitly] set; }
    }
}