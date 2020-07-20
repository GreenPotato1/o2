using System.Runtime.Serialization;

namespace Com.O2Bionics.AuditTrail.Contract
{
    [DataContract]
    public sealed class Facet
    {
        public Facet()
        {
        }

        public Facet(string id, string name = null, long count = 0)
        {
            Id = id;
            Name = name;
            Count = count;
        }

        [DataMember(Name = "Id")]
        public string Id { get; set; }

        [DataMember(Name = "Name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        ///     How many documents exist for the given <see cref="Id" /> and <see cref="Name" />.
        /// </summary>
        [DataMember(Name = "Count")]
        public long Count { get; set; }

        public override string ToString()
        {
            var result = $"{Id}, {Name}, {Count}";
            return result;
        }
    }
}