using System;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace Com.O2Bionics.Console.KibanaModels
{
    public sealed class SavedObject
    {
        public override string ToString()
        {
            return $"id={id}, type={type}";
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(id))
                throw new Exception($"{nameof(id)} must be not empty string, type='{type}'.");

            if (string.IsNullOrEmpty(type))
                throw new Exception($"{nameof(type)} must be not empty string, id='{id}'.");
        }
#pragma warning disable IDE1006 // Naming Styles
        public string id { get; [UsedImplicitly] set; }
        public string type { get; [UsedImplicitly] set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}