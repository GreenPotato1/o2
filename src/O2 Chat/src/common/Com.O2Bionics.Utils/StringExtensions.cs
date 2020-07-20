using System;
using System.Collections.Generic;
using Com.O2Bionics.Utils.Properties;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Com.O2Bionics.Utils
{
    public static class SettingsExtensions
    {
        public static List<string> AsStringList([CanBeNull] this string s)
        {
            return JsonConvert.DeserializeObject<List<string>>(string.IsNullOrWhiteSpace(s) ? "[]" : s) ?? new List<string>();
        }

        public static string LimitLength(this string value, int maxLength = 100)
        {
            if (maxLength <= 0)
                throw new ArgumentOutOfRangeException(string.Format(Resources.ArgumentMustBePositive2, nameof(maxLength), maxLength));

            if (string.IsNullOrEmpty(value))
                return value;
            if (value.Length <= maxLength)
                return value;

            const string ellipsis = "...";
            var result = value.Substring(0, maxLength) + ellipsis;
            return result;
        }

        [CanBeNull]
        public static string JoinAsString([CanBeNull] this string[] values, [NotNull] string separator = ",")
        {
            if (null == values || 0 == values.Length)
                return null;

            var result = string.Join(separator, values);
            return result;
        }

        [CanBeNull]
        public static string JoinAsString([CanBeNull] this ICollection<string> values, [NotNull] string separator = ",")
        {
            if (null == values || 0 == values.Count)
                return null;

            var result = string.Join(separator, values);
            return result;
        }

        [CanBeNull]
        public static string JoinAsString<T>([CanBeNull] this IEnumerable<T> values, [NotNull] string separator = ",")
        {
            if (null == values)
                return null;

            var result = string.Join(separator, values);
            return result;
        }
    }
}