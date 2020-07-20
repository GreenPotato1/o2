using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using pair = Com.O2Bionics.AuditTrail.Contract.IdName<uint>;

namespace Com.O2Bionics.AuditTrail.Contract
{
    /// <summary>
    ///     Store the change in a field, consisting of identifiers e.g. a list of department Ids.
    ///     The identifier is stored along with the entity name at the moment of saving.
    /// </summary>
    [DataContract]
    public sealed class IdListChange : INamed
    {
        public IdListChange()
        {
        }

        public IdListChange(
            [NotNull] string name,
            [CanBeNull] List<pair> deleted = null,
            [CanBeNull] List<pair> inserted = null)
        {
            Name = name;
            Deleted = deleted;
            Inserted = inserted;
        }

        [CanBeNull]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<pair> Deleted { get; set; }

        [CanBeNull]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<pair> Inserted { get; set; }

        [DataMember(IsRequired = true)]
        //[Text(Name = nameof(Name))]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}