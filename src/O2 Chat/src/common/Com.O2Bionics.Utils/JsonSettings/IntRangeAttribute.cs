using System;
using System.Collections.Generic;

namespace Com.O2Bionics.Utils.JsonSettings
{
    [Serializable]
    public class IntRangeAttribute : ValidationAttribute
    {
        public int? MinValue { get; }
        public int? MaxValue { get; }

        public IntRangeAttribute(object minValue = null, object maxValue = null)
        {
            MinValue = (int?)minValue;
            MaxValue = (int?)maxValue;
        }

        public override IEnumerable<string> Validate(object value)
        {
            if (value == null) yield break;

            if (value is int intValue)
            {
                if (intValue < MinValue)
                    yield return $"value should be not less than {MinValue} but is {intValue}";
                if (intValue > MaxValue)
                    yield return $"value should be not greater than {MaxValue} but is {intValue}";
            }
            else
            {
                yield return $"IntRangeAttribute can't be applied to fields of type {value.GetType().Name}";
            }
        }
    }
}