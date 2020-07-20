using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.Utils.Web.Settings
{
    [SettingsClass]
    public class AttachmentsSettings
    {
        [Default(1024000)]
        public int SizeLimit { get; set; }

        [Required]
        public AmazonUploadSettings Amazon { get; set; }
    }
}