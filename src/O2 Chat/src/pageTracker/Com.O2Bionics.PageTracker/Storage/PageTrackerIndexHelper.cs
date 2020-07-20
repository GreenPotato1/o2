using System.Collections.Generic;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.PageTracker.Storage
{
    public static class PageTrackerIndexHelper
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(PageTrackerIndexHelper));

        public static List<string> GetIndices([NotNull] PageTrackerSettings settings)
        {
            settings.NotNull(nameof(settings));

            return new List<string>
                {
                    settings.IdStorageIndex.Name,
                    settings.PageVisitIndex.Name,
                };
        }

        public static void DeleteIndices([NotNull] PageTrackerSettings settings)
        {
            settings.NotNull(nameof(settings));

            var client = new EsClient(settings.ElasticConnection);
            client.DeleteIndex(settings.IdStorageIndex.Name);
            client.DeleteIndex(settings.PageVisitIndex.Name);
        }

        public static void CreateIndices([NotNull] PageTrackerSettings settings)
        {
            settings.NotNull(nameof(settings));

            var client = new EsClient(settings.ElasticConnection);
            CreateIndicesImpl(client, settings);

            foreach (var scope in EnumHelper.Values<IdScope>())
                AddIdDocument(client, settings.IdStorageIndex.Name, scope);
        }

        /// <summary>
        /// After creating an index, call 
        /// <see cref="AddIdDocument"/>.
        /// </summary>
        private static void CreateIndicesImpl([NotNull] IEsClient client, [NotNull] PageTrackerSettings settings)
        {
            // TODO: should we check if the index exists before creating? will create overwrite existing?

            client.CreateIndex(
                settings.PageVisitIndex,
                d => d.Map<PageView>(
                    m => m.AutoMap(EsClient.MaxAutoMapRecursion)));
            client.CreateIndex(
                settings.IdStorageIndex,
                d => d.Map<IdStorageDoc>(
                    m => m.AutoMap(EsClient.MaxAutoMapRecursion)));
        }

        public static void AddIdDocument(
            [NotNull] IEsClient client,
            [NotNull] string indexName,
            IdScope scope,
            ulong initialValue = 0)
        {
            client.NotNull(nameof(client));
            indexName.IsCorrectEsIndexName(nameof(indexName));

            // TODO: p2. task-367. What if there are 2 or more nodes in a cluster - use 1 shard?
            // Also, "auto_expand_replicas": "0-all".

            var storeInitialValue = EsUnsignedHelper.ToEs(initialValue);

            var documentId = (int)scope;
            var doc = new IdStorageDoc { id = documentId, last = storeInitialValue };
            client.CreateDocument(indexName, doc);
            m_log.DebugFormat(
                "An id storage document with id={0} indexed in {1}",
                documentId,
                indexName);
        }
    }
}