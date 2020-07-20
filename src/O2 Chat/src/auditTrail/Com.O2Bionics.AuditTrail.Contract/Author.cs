using System.Globalization;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Contract
{
    [DataContract]
    public sealed class Author
    {
        public Author()
        {
        }

        public Author(string id, string name = null)
        {
            Id = id;
            Name = name;
        }

        public Author(long id, string name = null)
        {
            Id = id.ToString();
            Name = name;
        }

        public Author(decimal id, string name = null)
        {
            Id = id.ToString(CultureInfo.InvariantCulture);
            Name = name;
        }

        /// <summary>
        ///     Copy constructor.
        /// </summary>
        public Author([NotNull] Author value)
        {
            Id = value.Id;
            Name = value.Name;
        }

        [DataMember(Name = "Id")]
        //[Keyword(Name = "Id")]
        //[Text(Name = "Id")]
        public string Id { get; set; }

        [DataMember(Name = "Name")]
        //[Text(Name = "Name")]
        public string Name { get; set; }

        public override string ToString()
        {
            var result = $"Id={Id}, '{Name}'";
            return result;
        }
    }
}