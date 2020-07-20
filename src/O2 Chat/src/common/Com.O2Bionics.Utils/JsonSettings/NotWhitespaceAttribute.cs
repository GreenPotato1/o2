using System;
using System.Collections.Generic;

namespace Com.O2Bionics.Utils.JsonSettings
{
    [Serializable]
    public class NotWhitespaceAttribute : ValidationAttribute
    {
        public override IEnumerable<string> Validate(object value)
        {
            if (value == null) yield break;

            if (value is string stringValue)
            {
                if (string.IsNullOrWhiteSpace(stringValue))
                    yield return "value should not be whitespace";
            }
            else
            {
                yield return $"NotWhitespaceAttribute can't be applied to fields of type {value.GetType().Name}";
            }
        }
    }
}