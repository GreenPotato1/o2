using System;
using Com.O2Bionics.Utils.JsonSettings;

namespace Com.O2Bionics.Utils.Web.Settings
{
    [SettingsClass]
    public class AmazonUploadSettings
    {
        [Default("https://s3.amazonaws.com")]
        public Uri ServicesDomainUrl { get; set; }

        [Required]
        [NotWhitespace]
        public string BucketName { get; set; }

        [Default("Attachments")]
        [NotWhitespace]
        public string AttachmentsFolderName { get; set; }

        [Required]
        [NotWhitespace]
        public string AccessKey { get; set; }

        [Required]
        [NotWhitespace]
        public string SecretKey { get; set; }
    }
}