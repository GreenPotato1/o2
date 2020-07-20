using System;

namespace Com.O2Bionics.Utils.JsonSettings
{
    [Serializable]
    public class DefaultAttribute : Attribute
    {
        public object Value { get; }

        public DefaultAttribute(object value)
        {
            Value = value;
        }
    }
}