using System;
using System.Runtime.Serialization;

namespace Com.O2Bionics.AuditTrail.Contract
{
    [DataContract]
    public struct IdName<TIdentifier> : INamed
        where TIdentifier : IEquatable<TIdentifier>
    {
        [DataMember]
        public TIdentifier Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        public IdName(TIdentifier id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Id} {Name}";
        }

        public override int GetHashCode()
        {
            var a = Id?.GetHashCode() ?? 0;
            var b = Name?.GetHashCode() ?? 0;
            var result = a ^ b;
            return result;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IdName<TIdentifier> value)) return false;

            var result = ReferenceEquals(Id, value.Id) || null != Id && Id.Equals(value.Id);
            result = result && Name == value.Name;
            return result;
        }
    }
}