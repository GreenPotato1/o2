using System;
using System.Collections.Generic;
using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.FeatureService.Client
{
    [SettingsClass]
    public class FeatureServiceClientSettings
    {
        [Required]
        [NotWhitespace]
        public string ProductCode { get; set; }

        [Required]
        [NotEmpty]
        public IReadOnlyCollection<Uri> Urls { get; set; }

        [Default("0:0:5")]
        public TimeSpan Timeout { get; set; }

        [Required]
        [IntRange(0)]
        public int LocalCacheTimeToLiveSeconds { get; set; }

        public override string ToString()
        {
            return $"{ProductCode}, {nameof(Timeout)}={Timeout}, LocalCacheTTL={LocalCacheTimeToLiveSeconds} sec";
        }
    }
}