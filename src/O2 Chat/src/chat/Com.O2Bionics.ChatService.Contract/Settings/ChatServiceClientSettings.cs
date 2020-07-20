using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.ChatService.Contract.Settings
{
    [SettingsClass]
    public class ChatServiceClientSettings
    {
        [Default("127.0.0.1")]
        public string Host { get; set; }

        [Default(8523)]
        [IntRange(1)]
        public int Port { get; set; }
    }
}