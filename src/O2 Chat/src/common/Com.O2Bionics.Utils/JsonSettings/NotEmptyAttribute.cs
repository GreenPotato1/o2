using System;
using System.Collections.Generic;

namespace Com.O2Bionics.Utils.JsonSettings
{
    [Serializable]
    public class NotEmptyAttribute : ValidationAttribute
    {
        public override IEnumerable<string> Validate(object value)
        {
            if (value == null) yield break;

            if (value is string stringValue)
            {
                if (string.IsNullOrEmpty(stringValue))
                    yield return "value should not be empty string";
            }
            else if (value is IReadOnlyCollection<Uri> c1)
            {
                if (c1.Count == 0)
                    yield return "value should not be empty collection";
            }
            else if (value is IReadOnlyCollection<int> c2)
            {
                if (c2.Count == 0)
                    yield return "value should not be empty collection";
            }
            else if (value is IReadOnlyCollection<string> c3)
            {
                if (c3.Count == 0)
                    yield return "value should not be empty collection";
            }
            else if (value is IReadOnlyDictionary<string, string> d)
            {
                if (d.Count == 0)
                    yield return "value should not be empty dictionary";
            }
            else
            {
                yield return $"NotWhitespaceAttribute can't be applied to fields of type {value.GetType().Name}";
            }
        }
    }
}