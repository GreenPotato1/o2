using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Com.O2Bionics.AuditTrail.Contract.Utilities;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Nest;
using pair = Com.O2Bionics.AuditTrail.Contract.IdName<string>;

namespace Com.O2Bionics.AuditTrail.Contract
{
    /// <summary>
    ///     The root object of the change of type <typeparamref name="T" />.
    /// </summary>
    [DataContract]
    [ElasticsearchType(Name = FieldConstants.PreferredTypeName)]
    public sealed class AuditEvent<T>
    {
        public AuditEvent()
        {
        }

        /// <summary>
        ///     Copy constructor.
        /// </summary>
        public AuditEvent([NotNull] AuditEvent<T> value)
        {
            if (null == value)
                throw new ArgumentNullException(nameof(value));

            Id = value.Id;
            Timestamp = value.Timestamp;
            Operation = value.Operation;
            Status = value.Status;
            if (null != value.Author)
                Author = new Author(value.Author);

            CustomerId = value.CustomerId;
            FieldChanges = value.FieldChanges;
            CustomValues = value.CustomValues;
            ObjectNames = value.ObjectNames;
            CustomObjects = value.CustomObjects;

            All = value.All;
            Changed = value.Changed;

            OldValue = value.OldValue;
            NewValue = value.NewValue;
        }

        [DataMember]
        [Keyword(Name = nameof(Id))]
        public Guid Id { get; set; }

        [DataMember(IsRequired = true)]
        [Date(Name = nameof(Timestamp))]
        public DateTime Timestamp { get; set; }

        [DataMember(IsRequired = true)]
        public string Operation { get; set; }

        [DataMember(IsRequired = true)]
        public string Status { get; set; }

        /// <summary>
        ///     Who made the changes.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Author Author { get; set; }

        [DataMember]
        //[Keyword(Name = nameof(ServiceConstants.CustomerId))]
        public string CustomerId { get; set; }


        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public T OldValue { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public T NewValue { get; set; }


        // Full text search fields.

        /// <summary>
        ///     Meaningful fields, excluding specified in explicit filters: <seealso cref="Timestamp" />,
        ///     <seealso cref="Operation" />, <seealso cref="Status" />, <seealso cref="Author" />.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [Text(Name = nameof(All))]
        public string All { get; set; }

        /// <summary>
        ///     Changed fields:
        ///     1) When both old and new instances are not null, only the changed values taken from <seealso cref="FieldChanges" />
        ///     .
        ///     2) Otherwise, it is equal to <seealso cref="All" />.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [Text(Name = nameof(Changed))]
        public string Changed { get; set; }

        public override string ToString()
        {
            const char separator = ' ';
            var count = FieldChanges?.Count();

            var result =
                Id.ToString()
                + separator + (null == count ? "No changes" : $"{count.Value} changes")
                + separator + Status
                + separator + Operation
                + separator + Timestamp
                + separator + Author
                + separator + "Customer=" + CustomerId;
            return result;
        }


        #region Properties, not mapped in Elastic.

        // Dictionary must be explicitly included (by default Elastic creates incorrect mapping e.g. "Dictionary.Comparer" is unnecessarily included).

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public FieldChanges FieldChanges { get; set; }

        /// <summary>
        ///     Set of key/value pairs e.g. User Id, User name, etc.
        /// </summary>
        [CanBeNull]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<string, string> CustomValues { get; set; }

        /// <summary>
        ///     Set of dictionaries of key/value pairs.
        ///     E.g. a set of department id-name pairs, plus a similar set for users.
        ///     It is serialized as <seealso cref="CustomObjects"/>
        ///     because the dictionary keys are serialized as field names, and Elastic Search has a limit on the number of fields.
        ///     For example, {"123": "Department name"} will be serialized as
        ///     {"Id": "123", "Name": "Department name"}
        /// </summary>
        [CanBeNull]
        [IgnoreDataMember]
        public Dictionary<string, Dictionary<string, string>> ObjectNames { get; set; }

        /// <summary>
        ///     Used to serialize <seealso cref="ObjectNames"/>.
        /// </summary>
        [CanBeNull]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Dictionary<string, List<pair>> CustomObjects
        {
            get => ObjectNames.ToListOfPairs();
            set => ObjectNames = value.FromListOfPairs();
        }

        #endregion
    }
}