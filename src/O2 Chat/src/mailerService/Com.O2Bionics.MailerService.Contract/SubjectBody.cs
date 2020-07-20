using System.Runtime.Serialization;

namespace Com.O2Bionics.MailerService.Contract
{
    [DataContract]
    public sealed class SubjectBody
    {
        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string Body { get; set; }

        public override string ToString()
        {
            return $"{nameof(Subject)}='{Subject}', {nameof(Body)}='{Body}'";
        }
    }
}