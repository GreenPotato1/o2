using System.Runtime.Serialization;
using Nest;

namespace Com.O2Bionics.PageTracker.Contract
{
    [DataContract]
    public sealed class TimeZoneDescription
    {
        [DataMember]
        [Number(NumberType.Integer, Index = false)]
        public int Offset { get; set; }

        [DataMember]
        [Keyword(IgnoreAbove = PageTrackerConstants.TimeZoneDescriptionMaxLength, Index = false)]
        public string Description { get; set; }

        public TimeZoneDescription()
        {
        }

        public TimeZoneDescription(int offset, string description)
        {
            Offset = offset;
            Description = description;
        }

        public override string ToString()
        {
            return $"Offset={Offset}, Description={Description}";
        }
    }
}