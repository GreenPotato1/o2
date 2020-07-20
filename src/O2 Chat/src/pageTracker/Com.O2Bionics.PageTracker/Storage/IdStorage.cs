using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Nest;

namespace Com.O2Bionics.PageTracker.Storage
{
    public sealed class IdStorage : IIdStorage
    {
        private readonly IEsClient m_client;
        private readonly string m_indexName;
        private readonly ulong m_blockSize;

        public IdStorage([NotNull] PageTrackerSettings settings, [NotNull] IEsClient client)
        {
            settings.NotNull(nameof(settings));

            m_client = client.NotNull(nameof(client));
            m_indexName = settings.IdStorageIndex.Name;
            m_blockSize = (ulong)settings.IdStorageBlockSize
                .MustBePositive(nameof(settings.IdStorageBlockSize));

            if (!client.IndexExists(m_indexName))
            {
                throw new Exception(
                    $"Id storage index {client.ClusterName}/'{m_indexName}' doesn't exists");
            }

            foreach (var scope in EnumHelper.Values<IdScope>())
            {
                if (m_client.Get<IdStorageDoc>(m_indexName, (int)scope) == null)
                {
                    throw new Exception(
                        $"Id storage document for {scope:G}({scope:D}) doesn't exist in {client.ClusterName}/{m_indexName}");
                }
            }
        }

        public async Task<ulong> Add(IdScope scope)
        {
            const string block = "block";

            var documentId = (int)scope;
            var entry = await m_client.UpdateAsync<IdStorageDoc>(
                m_indexName,
                documentId,
                r => r.Script = new InlineScript(
                    $"ctx._source.{nameof(IdStorageDoc.last)} += params.{block}")
                    {
                        Lang = "painless",
                        Params = new Dictionary<string, object> { { block, m_blockSize } },
                    });
            return EsUnsignedHelper.FromEs(entry.last);
        }

        public ulong BlockSize => m_blockSize;
    }
}