using System;
using System.Collections.Generic;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Client.Settings
{
    [SettingsClass]
    [SettingsRoot(RootName)]
    [UsedImplicitly]
    public sealed class AuditTrailClientSettings
    {
        public const string RootName = "auditTrailClient";

        [Required]
        [NotEmpty]
        public IReadOnlyCollection<Uri> Urls { get; [UsedImplicitly] set; }
    }
}