using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.O2Bionics.Utils.JsonSettings;
using Elasticsearch.Net;
using JetBrains.Annotations;
using Nest;

namespace Com.O2Bionics.Elastic
{
    public interface IEsClient
    {
        string ClusterName { get; }

        void Flush([NotNull] string index);

        bool IndexExists(string index);
        bool CreateIndex([NotNull] EsIndexSettings index, [NotNull] Func<MappingsDescriptor, IPromise<IMappings>> func);
        void DeleteIndex([NotNull] string index);

        T Get<T>(string index, Id id)
            where T : class;

        Task<ISearchResponse<T>> SearchAsync<T>(
            [NotNull] string indexName,
            [NotNull] Func<SearchDescriptor<T>, ISearchRequest> selector)
            where T : class;

        KeyValuePair<string, T> FetchFirstDocument<T>(
            [NotNull] string indexName,
            [NotNull] string entityType = "doc",
            [CanBeNull] Func<QueryContainerDescriptor<T>, QueryContainer> query = null,
            int timeoutMilliseconds = 20000)
            where T : class;

        Task CreateDocument<T>(string index, T doc)
            where T : class;

        Task<StringResponse> Index([NotNull] string index, [NotNull] string json);

        IBulkResponse IndexMany<T>([NotNull] string index, IList<T> values)
            where T : class;

        Task<T> UpdateAsync<T>(string index, Id id, Action<IUpdateRequest<T, T>> selector)
            where T : class;

        [Pure]
        IGetMappingResponse GetMapping<T>([NotNull] string index)
            where T : class;
    }
}