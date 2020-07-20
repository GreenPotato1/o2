using System;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.AuditTrail.Contract.Names
{
    public static class IndexNameFormatter
    {
        [NotNull]
        public static string Format([NotNull] string index, [NotNull] string productCode)
        {
            var result = $"{index}_{productCode}";
            return result;
        }

        [NotNull]
        public static string FormatWithValidation([NotNull] string index, [NotNull] string productCode)
        {
            if (string.IsNullOrEmpty(index))
                throw new ArgumentNullException(nameof(index));
            if (string.IsNullOrEmpty(productCode))
                throw new ArgumentNullException(nameof(productCode));

            var result = Format(index, productCode);
            var error = IdentifierHelper.LowerCase(result);
            if (!string.IsNullOrEmpty(error))
                throw new Exception($"Index '{result}' is bad: {error}");
            return result;
        }
    }
}