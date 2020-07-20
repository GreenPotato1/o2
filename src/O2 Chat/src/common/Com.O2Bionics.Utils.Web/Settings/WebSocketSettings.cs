using System;
using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.Utils.Web.Settings
{
    [SettingsClass]
    public class WebSocketSettings
    {
        [Default("0:1:50")]
        public TimeSpan ConnectionTimeout { get; set; }

        [Default("0:1:30")]
        public TimeSpan DisconnectTimeout { get; set; }

        [Default("0:0:30")]
        public TimeSpan KeepAlive { get; set; }
    }
}