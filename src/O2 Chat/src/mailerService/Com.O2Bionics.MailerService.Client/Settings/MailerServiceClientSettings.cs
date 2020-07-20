using System;
using System.Collections.Generic;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.MailerService.Client.Settings
{
    [SettingsClass]
    [SettingsRoot(RootName)]
    [UsedImplicitly]
    public sealed class MailerServiceClientSettings
    {
        public const string RootName = "mailerServiceClient";

        [Required]
        [NotEmpty]
        public IReadOnlyCollection<Uri> Urls { get; [UsedImplicitly] set; }
    }
}