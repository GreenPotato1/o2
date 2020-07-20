using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    //.NET Framework should have the ConcurrentHashSet class.
    public sealed class ConcurrentHashSet<T> : ConcurrentDictionary<T, byte>
    {
        public ConcurrentHashSet()
        {
        }

        public ConcurrentHashSet([CanBeNull] IEnumerable<T> hashSet)
        {
            if (null == hashSet)
                return;
            foreach (var key in hashSet)
            {
                TryAdd(key, 0);
            }
        }

        public ConcurrentHashSet([CanBeNull] T key)
        {
            if (null == key)
                return;
            TryAdd(key, 0);
        }

        public bool TryAdd(T key)
        {
            var result = TryAdd(key, 0);
            return result;
        }

        public bool TryRemove(T key)
        {
            var result = TryRemove(key, out var _);
            return result;
        }

        [CanBeNull]
        public HashSet<T> ToHashSet()
        {
            return new HashSet<T>(Keys.ToList());
        }

        [NotNull]
        public T[] KeysToArray()
        {
            return Keys.ToArray();
        }
    }
}