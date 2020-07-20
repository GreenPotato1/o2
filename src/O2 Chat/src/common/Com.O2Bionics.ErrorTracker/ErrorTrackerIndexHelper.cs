using Com.O2Bionics.Elastic;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;

namespace Com.O2Bionics.ErrorTracker
{
    public static class ErrorTrackerIndexHelper
    {
        public static void CreateIndex([NotNull] IEsClient client, [NotNull] EsIndexSettings indexSettings)
        {
            client.NotNull(nameof(client));
            indexSettings.NotNull(nameof(indexSettings));
            indexSettings.Name.IsCorrectEsIndexName(nameof(indexSettings.Name));

            client.CreateIndex(
                indexSettings,
                d => d.Map<ErrorInfo>(
                    m => m.AutoMap(EsClient.MaxAutoMapRecursion)
                        .SetLongStringsSize()));
        }
    }
}