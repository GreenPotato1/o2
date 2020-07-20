using System;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract.Widget
{
    [DataContract]
    public sealed class WidgetDailyViewCountExceededEvent
    {
        [DataMember]
        public long Total { get; set; }

        [DataMember]
        public long Limit { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        public override string ToString()
        {
            return $"Tot {Total}, Lim {Limit}, {Date}";
        }
    }
}