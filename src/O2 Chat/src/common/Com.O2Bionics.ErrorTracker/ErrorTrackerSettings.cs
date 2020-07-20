using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.ErrorTracker
{
    [SettingsClass]
    [SettingsRoot(RootName)]
    public sealed class ErrorTrackerSettings
    {
        public const string RootName = "errorTracker";

        [Required]
        [NotWhitespace]
        public string EmergencyLogDirectory { get; [UsedImplicitly] set; }

        [Required]
        public EsConnectionSettings ElasticConnection { get; [UsedImplicitly] set; }

        [Required]
        public EsIndexSettings Index { get; [UsedImplicitly] set; }
    }
}