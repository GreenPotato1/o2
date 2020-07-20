using Com.O2Bionics.AuditTrail.Contract.Settings;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Web
{
    [SettingsRoot(AuditTrailServiceSettings.RootName)]
    [UsedImplicitly]
    public sealed class AuditTrailSettings
    {
        [Required]
        [SettingsRoot("errorTracker")]
        public ErrorTrackerSettings ErrorTracker { get; [UsedImplicitly] set; }

        [Required]
        [SettingsRoot(AuditTrailServiceSettings.RootName)]
        public AuditTrailServiceSettings AuditTrailServiceSettings { get; [UsedImplicitly] set; }
    }
}