using System.Runtime.Serialization;
using JetBrains.Annotations;
using Nest;

namespace Com.O2Bionics.PageTracker.Contract
{
    [DataContract]
    public sealed class GeoLocation
    {
        [DataMember]
        [Keyword(IgnoreAbove = 64)]
        public string Country { get; set; }

        [DataMember]
        [Keyword(IgnoreAbove = 64)]
        public string City { get; set; }

        [DataMember]
        [GeoPoint]
        [CanBeNull]
        public Point Point { get; set; }

        public override string ToString()
        {
            var lat = Format(nameof(Point.lat), Point?.lat);
            var lon = Format(nameof(Point.lon), Point?.lon);

            return $"Country={Country}, City={City}{lat}{lon}";
        }

        private static string Format(string name, double? value)
        {
            if (!value.HasValue)
                return null;

            var result = ", " + name + "=" + value.Value;
            return result;
        }
    }
}