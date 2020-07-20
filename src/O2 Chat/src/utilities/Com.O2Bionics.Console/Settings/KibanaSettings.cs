using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.Console.Settings
{
    [SettingsClass]
    [SettingsRoot(RootName)]
    [UsedImplicitly]
    public sealed class KibanaSettings
    {
        private const string RootName = "kibana";

        [Required]
        [NotWhitespace]
        public string KibanaUrl { get; [UsedImplicitly] set; }
    }
}