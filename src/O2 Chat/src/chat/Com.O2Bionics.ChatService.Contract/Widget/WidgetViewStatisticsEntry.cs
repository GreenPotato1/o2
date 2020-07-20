using System;
using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract.Widget
{
    [DataContract]
    public sealed class WidgetViewStatisticsEntry
    {
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public uint CustomerId { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DateTime Date { get; set; }

        [DataMember]
        public long Count { get; set; }

        [DataMember]
        public bool IsViewCountExceeded { get; set; }

        public override string ToString()
        {
            return $"{CustomerId} {Date} {Count}";
        }
    }
}