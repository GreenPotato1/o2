using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.MailerService.Web
{
    [UsedImplicitly]
    [SettingsRoot(RootName)]
    public sealed class MailerServiceSettings
    {
        private const string RootName = "mailerService";

        [Required]
        public SmtpClientSettings Smtp { get; [UsedImplicitly] set; }

        [Required]
        [SettingsRoot(ErrorTrackerSettings.RootName)]
        public ErrorTrackerSettings ErrorTracker { get; [UsedImplicitly] set; }
    }
}