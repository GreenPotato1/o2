using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils.JsonSettings
{
    public class EsConnectionSettings
    {
        public EsConnectionSettings()
        {
        }

        public EsConnectionSettings([NotNull] [ItemNotNull] params Uri[] uris)
        {
            if (uris == null) throw new ArgumentNullException(nameof(uris));
            if (uris.Length == 0) throw new ArgumentException("Can't be empty", nameof(uris));
            if (uris.Contains(null)) throw new ArgumentException("Can't contain nulls", nameof(uris));

            Uris = uris.ToList();
        }

        [Required]
        [NotEmpty]
        public IReadOnlyCollection<Uri> Uris { get; [UsedImplicitly] set; }

        public override string ToString()
        {
            return $"[{Uris.JoinAsString()}]";
        }
    }
}