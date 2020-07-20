using System.Runtime.Serialization;
using Com.O2Bionics.AuditTrail.Contract;

namespace Com.O2Bionics.AuditTrail
{
    [DataContract]
    public sealed class AuditEventShort
    {
        //Note. Elastic converts names to the lower case - the correct names must be specified.

        [DataMember(Name = "Author")]
        public Author Author { get; set; }
    }
}