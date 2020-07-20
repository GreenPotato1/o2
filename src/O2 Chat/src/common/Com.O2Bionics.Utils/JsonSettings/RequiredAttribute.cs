using System;
using System.Collections.Generic;

namespace Com.O2Bionics.Utils.JsonSettings
{
    [Serializable]
    public class RequiredAttribute : ValidationAttribute
    {
        public override IEnumerable<string> Validate(object value)
        {
            if (value == null) yield return "value is required";
        }
    }
}