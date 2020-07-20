using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable InconsistentNaming

namespace Com.O2Bionics.Console.KibanaModels
{
    public sealed class SavedObjectReport
    {
#pragma warning disable IDE1006 // Naming Styles
        public List<SavedObject> saved_objects { get; [UsedImplicitly] set; }
#pragma warning restore IDE1006 // Naming Styles

        public override string ToString()
        {
            if (null == saved_objects || 0 == saved_objects.Count)
                return "No saved_objects";

            return $"{saved_objects.Count} saved_objects";
        }
    }
}