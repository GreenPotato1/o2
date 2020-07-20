using System;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract.Widget
{
    [DataContract]
    public class WidgetUnknownDomainTooManyEvent
    {
        [DataMember]
        public string Domains { get; set; }

        [DataMember]
        public int Limit { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        public override string ToString()
        {
            return $"{nameof(Domains)}='{Domains}', {Date}";
        }
    }
}