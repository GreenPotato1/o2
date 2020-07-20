using System;
using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.Utils.Web.Settings
{
    [SettingsClass]
    [SettingsRoot("pageTrackerClient")]
    public sealed class PageTrackerClientSettings
    {
        [Required]
        public Uri Url { get; set; }
    }
}