using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    public static class EnumHelper
    {
        [Pure]
        [NotNull]
        public static IEnumerable<T> Values<T>() where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException($"{typeof(T).FullName} must be an enumeration type", nameof(T));

            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}