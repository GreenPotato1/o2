using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.O2Bionics.Utils.JsonSettings
{
    [Serializable]
    public class ElasticIndexNameAttribute : ValidationAttribute
    {
        public override IEnumerable<string> Validate(object value)
        {
            if (value == null) yield break;

            if (value is string stringValue)
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                    yield return "value can't be empty or whitespace";
                var lower = stringValue.ToLowerInvariant();
                if (lower != stringValue)
                    yield return "value must be all lowercase";

                if (!IdentifierHelper.Alpha(lower[0]))
                    yield return "value must start with a letter";

                if (lower.Any(c => !IdentifierHelper.Alpha(c) && !IdentifierHelper.Digit(c) && c != '_'))
                    yield return "value must contain only letters, digits or underscore";
            }
            else
            {
                yield return $"ElasticIndexNameAttribute can't be applied to fields of type {value.GetType().Name}";
            }
        }
    }
}