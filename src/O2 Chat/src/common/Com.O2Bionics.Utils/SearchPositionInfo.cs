using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    [DataContract]
    public sealed class SearchPositionInfo
    {
        [DataMember(IsRequired = true)]
        public List<string> Values { get; set; }

        public SearchPositionInfo()
        {
            Values = new List<string>();
        }

        public SearchPositionInfo(IReadOnlyCollection<object> values)
        {
            values.NotNull(nameof(values));
            values.NotEmpty(nameof(values));

            Values = values.Select(x => x.ToString()).ToList();
        }

        [CanBeNull]
        public List<object> AsObjectList()
        {
            return Values.Cast<object>().ToList();
        }

        public override string ToString()
        {
            return $"{Values.JoinAsString()}";
        }
    }
}