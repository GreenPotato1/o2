using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Contract
{
    /// <summary>
    ///     Store the change in a field having a plain type such as:
    ///     Numbers: bool, char, short, int, long, decimal, float and double, date and time.
    ///     Strings: string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public sealed class PlainFieldChange<T> : INamed
    {
        public PlainFieldChange()
        {
        }

        public PlainFieldChange([NotNull] string name, [CanBeNull] T oldValue, [CanBeNull] T newValue)
        {
            Name = name;
            OldValue = oldValue;
            NewValue = newValue;
        }

        [CanBeNull]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public T OldValue { get; set; }

        [CanBeNull]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public T NewValue { get; set; }

        [DataMember(IsRequired = true)]
        //[Text(Name = nameof(Name))]
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name}: {OldValue} -> {NewValue}";
        }
    }
}