using System.Collections.Specialized;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    /// <summary>
    /// Extension methods for <seealso cref="NameValueCollection"/>.
    /// </summary>
    public static class NameValueCollectionExtensions
    {
        public static ulong GetULong([CanBeNull] this NameValueCollection[] collections, [NotNull] string key)
        {
            if (null != collections)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < collections.Length; i++)
                {
                    if (null == collections[i])
                        continue;

                    var result = GetULong(collections[i], key);
                    if (0 < result)
                        return result;
                }
            }

            return 0;
        }

        public static uint GetUint([CanBeNull] this NameValueCollection[] collections, [NotNull] string key)
        {
            if (null != collections)
            {
                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < collections.Length; i++)
                {
                    if (null == collections[i])
                        continue;

                    var result = GetUint(collections[i], key);
                    if (0 < result)
                        return result;
                }
            }

            return 0;
        }

        private static ulong GetULong([NotNull] this NameValueCollection collection, [NotNull] string key)
        {
            var value = collection[key];
            if (ulong.TryParse(value, out var result) && 0 < result)
                return result;

            return 0;
        }

        public static uint GetUint([NotNull] this NameValueCollection collection, [NotNull] string key)
        {
            var value = collection[key];
            if (uint.TryParse(value, out var result) && 0 < result)
                return result;

            return 0;
        }
    }
}