using System.Collections.Generic;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils.JsonSettings
{
    public class EsIndexSettings
    {
        [Required]
        [ElasticIndexName]
        public string Name { get; set; }

        public IReadOnlyDictionary<string, string> Settings { get; set; }

        public EsIndexSettings()
        {
        }

        public EsIndexSettings([NotNull] EsIndexSettings indexSettings, [NotNull] string name)
        {
            indexSettings.NotNull(nameof(indexSettings));
            name.NotNull(nameof(name));

            Name = name;
            Settings = indexSettings.Settings;
        }
    }
}