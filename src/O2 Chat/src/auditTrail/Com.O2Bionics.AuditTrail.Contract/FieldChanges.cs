using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Contract
{
    [DataContract]
    [DebuggerDisplay("{Count()}")]
    public sealed class FieldChanges
    {
        [CanBeNull] [DataMember(IsRequired = false, EmitDefaultValue = false)] public List<PlainFieldChange<bool>> BoolChanges;

        [CanBeNull] [DataMember(IsRequired = false, EmitDefaultValue = false)] public List<PlainFieldChange<DateTime>> DateTimeChanges;

        /// <summary>
        ///     For int, long and decimal.
        /// </summary>
        [CanBeNull] [DataMember(IsRequired = false, EmitDefaultValue = false)] public List<PlainFieldChange<decimal>> DecimalChanges;

        /// <summary>
        ///     For lists of Identifiers.
        ///     E.g. a user can belong to several departments/channels.
        /// </summary>
        [CanBeNull] [DataMember(IsRequired = false, EmitDefaultValue = false)] public List<IdListChange> IdListChanges;

        /// <summary>
        ///     Enumeration value is displayed as a string.
        /// </summary>
        [CanBeNull] [DataMember(IsRequired = false, EmitDefaultValue = false)] public List<PlainFieldChange<string>> StringChanges;

        /// <summary>
        ///     For lists of strings.
        ///     E.g. a customer can own several domains.
        /// </summary>
        [CanBeNull] [DataMember(IsRequired = false, EmitDefaultValue = false)] public List<ListChange<string>> StringListChanges;

        public int Count()
        {
            var list = new ICollection[]
                {
                    BoolChanges,
                    DateTimeChanges,
                    DecimalChanges,
                    StringChanges,
                    IdListChanges,
                    StringListChanges
                };

            var result = list.Select(a => a?.Count ?? 0).Sum();
            return result;
        }
    }
}