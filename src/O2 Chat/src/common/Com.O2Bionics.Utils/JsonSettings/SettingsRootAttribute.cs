using System;

namespace Com.O2Bionics.Utils.JsonSettings
{
    [Serializable]
    public class SettingsRootAttribute : Attribute
    {
        public string Name { get; }

        public SettingsRootAttribute(string name)
        {
            Name = name;
        }
    }
}