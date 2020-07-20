using System;
using System.Collections.Generic;

namespace Com.O2Bionics.Utils.JsonSettings
{
    public abstract class ValidationAttribute : Attribute
    {
        public abstract IEnumerable<string> Validate(object value);
    }
}