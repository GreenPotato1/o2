using System.Runtime.Serialization;

namespace Com.O2Bionics.ChatService.Contract.Widget
{
    [DataContract]
    public sealed class WidgetUnknownDomain
    {
        [DataMember]
        public string Domains { get; set; }

        [DataMember]
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name} of {Domains}";
        }
    }
}