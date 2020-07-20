using System;
using System.Collections.Generic;

namespace Com.O2Bionics.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Section<T>(this IEnumerable<T> source, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var section = new List<T>(length);

            foreach (var item in source)
            {
                section.Add(item);
                if (section.Count != length) continue;

                yield return section.AsReadOnly();
                section = new List<T>(length);
            }

            if (section.Count > 0)
                yield return section.AsReadOnly();
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(source, comparer);
        }
    }
}