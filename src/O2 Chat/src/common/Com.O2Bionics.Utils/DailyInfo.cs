using System.Collections.Concurrent;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    public sealed class DailyInfo<T>
    {
        public readonly long Days;

        //Values are only added and updated.
        [NotNull] public readonly ConcurrentDictionary<uint, T> KeyValues;

        public DailyInfo(long days, [CanBeNull] ConcurrentDictionary<uint, T> value = null)
        {
            Debug.Assert(0 <= days);
            Days = days;
            KeyValues = value ?? new ConcurrentDictionary<uint, T>();
        }

        public override string ToString()
        {
            return $"{nameof(Days)}={Days}, {KeyValues.Count} entries.";
        }
    }
}