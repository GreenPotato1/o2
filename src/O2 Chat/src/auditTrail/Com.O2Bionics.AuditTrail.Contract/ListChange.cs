using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Contract
{
    /// <summary>
    ///     Store the change in a field, consisting of possibly several primitives e.g. a list of strings.
    /// </summary>
    [DataContract]
    public sealed class ListChange<T> : INamed
    {
        public ListChange()
        {
        }

        public ListChange(
            [NotNull] string name,
            [CanBeNull] List<T> deleted = null,
            [CanBeNull] List<T> inserted = null)
        {
            Name = name;
            Deleted = deleted;
            Inserted = inserted;
        }

        [CanBeNull]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<T> Deleted { get; set; }

        [CanBeNull]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<T> Inserted { get; set; }

        [DataMember(IsRequired = true)]
        //[Text(Name = nameof(Name))]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}