using JetBrains.Annotations;

namespace Com.O2Bionics.Utils.JsonSettings
{
    public static class IdentifierHelper
    {
        /// <summary>
        /// Return an error if the <paramref name="value"/> does not match "[a-zA-Z]{1}[a-zA-Z_0-9]*".
        /// </summary>
        [CanBeNull]
        // TODO: split checks: first alpha and case
        public static string LowerOrUpperCase([CanBeNull] string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (!Alpha(value[0]))
                return "value must start with a..z or A..Z.";

            for (var i = 1; i < value.Length; i++)
            {
                if (!AlphaOrUnderscore(value[i]) && !Digit(value[i]))
                    return Properties.Resources.LowerOrUpperCaseError;
            }

            return null;
        }

        /// <summary>
        /// Return an error if the <paramref name="value"/> does not match "[a-z]{1}[a-z_0-9]*".
        /// </summary>
        [CanBeNull]
        // TODO: split checks: first alpha and case
        public static string LowerCase([CanBeNull] string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (!AlphaLower(value[0]))
                return "value must start with a..z";

            for (var i = 1; i < value.Length; i++)
            {
                if (!AlphaLowerOrUnderscore(value[i]) && !Digit(value[i]))
                    return "value must start with a..z and consist of a..z and digits 0..9";
            }

            return null;
        }

        private static bool AlphaLower(char c)
        {
            var result = 'a' <= c && c <= 'z';
            return result;
        }

        public static bool Alpha(char c)
        {
            var result = 'a' <= c && c <= 'z' || 'A' <= c && c <= 'Z';
            return result;
        }

        private static bool AlphaLowerOrUnderscore(char c)
        {
            var result = AlphaLower(c) || '_' == c;
            return result;
        }

        private static bool AlphaOrUnderscore(char c)
        {
            var result = Alpha(c) || '_' == c;
            return result;
        }

        public static bool Digit(char c)
        {
            var result = '0' <= c && c <= '9';
            return result;
        }
    }
}