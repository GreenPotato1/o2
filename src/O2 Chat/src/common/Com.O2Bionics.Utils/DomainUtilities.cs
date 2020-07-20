using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils
{
    public static class DomainUtilities
    {
        public const int DefaultMaximumUnknownDomains = 100;

        public const char DomainSeparator = ';';

        [CanBeNull]
        [Pure]
        public static string[] GetDomains([CanBeNull] string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return null;

            var list = raw.Split(new[] { DomainSeparator }, StringSplitOptions.RemoveEmptyEntries);
            var result = 0 < list.Length ? list : null;
            return result;
        }

        [Pure]
        public static bool HasDomain([NotNull] string[] domains, [NotNull] string name)
        {
            Debug.Assert(null != domains && 0 < domains.Length, nameof(domains));
            Debug.Assert(!string.IsNullOrEmpty(name), nameof(name));

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < domains.Length; i++)
            {
                var domain = domains[i];
                if (name.EndsWith(domain, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        //It is faster to use the other "HasDomain" method.
        [Pure]
        public static bool HasDomain([CanBeNull] string domainList, [NotNull] string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), nameof(name));
            if (string.IsNullOrEmpty(domainList))
                return false;

            if (name.EndsWith(domainList, StringComparison.OrdinalIgnoreCase))
                return true;

            var domains = domainList.Split(new[] { DomainSeparator }, StringSplitOptions.RemoveEmptyEntries);
            var result = 0 < domains.Length && HasDomain(domains, name);
            return result;
        }
    }
}