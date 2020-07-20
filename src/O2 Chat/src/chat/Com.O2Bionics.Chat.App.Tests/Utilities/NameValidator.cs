using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Com.O2Bionics.Chat.App.Tests.Utilities
{
    public static class NameValidator
    {
        /// <summary>
        ///     Return true when valid.
        /// </summary>
        public static bool ValidateServerName([NotNull] string server, bool throwOnException = true)
        {
            if (string.IsNullOrEmpty(server))
            {
                if (throwOnException)
                    throw new ArgumentNullException(nameof(server));
                return false;
            }

            var regex = new Regex(@"^[a-zA-Z0-9]+[a-zA-Z\.\-0-9]*[a-zA-Z0-9]+$");
            if (regex.IsMatch(server))
                return true;

            if (throwOnException)
                throw new ArgumentException($"The argument '{nameof(server)}' must be a valid server name.");
            return false;
        }
    }
}