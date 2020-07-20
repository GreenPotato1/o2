using System.Runtime.Serialization;

namespace Com.O2Bionics.MailerService.Contract
{
    [DataContract]
    public sealed class MailRequest
    {
        [DataMember(IsRequired = true)]
        public string ProductCode { get; set; }

        [DataMember(IsRequired = true)]
        public string TemplateId { get; set; }

        /// <summary>
        /// JSON serialized data.
        /// </summary>
        [DataMember]
        public string TemplateModel { get; set; }

        [DataMember]
        public string To { get; set; }

        public override string ToString()
        {
            return $"Product='{ProductCode}', Id='{TemplateId}', To='{To}', Model='{TemplateModel}'";
        }
    }
}