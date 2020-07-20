using System.Runtime.Serialization;

namespace Com.O2Bionics.MailerService
{
    [DataContract]
    public sealed class MailRequest
    {
        [DataMember(IsRequired = true, Name = "productCode")]
        public string ProductCode { get; set; }

        [DataMember(IsRequired = true, Name = "templateId")]
        public string TemplateId { get; set; }

        /// <summary>
        /// JSON serialized data.
        /// </summary>
        [DataMember(Name = "templateModel")]
        public string TemplateModel { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }
    }
}