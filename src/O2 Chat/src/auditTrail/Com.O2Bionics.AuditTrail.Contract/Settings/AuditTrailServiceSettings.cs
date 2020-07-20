using System.Collections.Generic;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Contract.Settings
{
    /// <summary>
    ///     The actual index is Storage.Index plus the product code.
    /// </summary>
    [SettingsClass]
    [SettingsRoot(RootName)]
    [UsedImplicitly]
    public sealed class AuditTrailServiceSettings
    {
        public const string RootName = "auditTrailService";

        public const int RequestMaxLength = 8 * 1024 * 1024;

        [Required]
        [NotEmpty]
        public IReadOnlyCollection<string> ProductCodes { get; [UsedImplicitly] set; }

        [Required]
        public EsConnectionSettings ElasticConnection { get; [UsedImplicitly] set; }

        [Required]
        public EsIndexSettings Index { get; [UsedImplicitly] set; }
    }
}