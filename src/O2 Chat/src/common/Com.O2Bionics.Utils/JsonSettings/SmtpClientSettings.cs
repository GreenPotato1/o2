// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Com.O2Bionics.Utils.JsonSettings
{
    [SettingsClass]
    public sealed class SmtpClientSettings
    {
        [Required]
        [NotWhitespace]
        public string Host { get; set; }

        [Default(25)]
        [IntRange(1)]
        public int Port { get; set; }

        [Default("no-respond@o2bionics.com")]
        [NotWhitespace]
        public string From { get; set; }
    }
}