using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using Elasticsearch.Net;
using JetBrains.Annotations;
using log4net;
using Nest;

namespace Com.O2Bionics.Elastic
{
    public sealed class EsClient : IEsClient
    {
        public static bool Trace = false;

        public const int MaxAutoMapRecursion = 10;
        public const int UpdateRetryOnConflict = 100;

        private static readonly ILog m_log = LogManager.GetLogger(typeof(EsClient));

        internal ElasticClient Client { get; }
        public string ClusterName { get; }

        public EsClient([NotNull] EsConnectionSettings settings)
        {
            settings.NotNull(nameof(settings));

            Client = new ElasticClient(BuildConnectionSettings(settings));

            var r = Client.ClusterState();
            if (!r.IsValid)
                throw new EsException($"ClusterState call failed for {settings}: {r.BuildErrorMessage()}");
            ClusterName = r.ClusterName;

            m_log.DebugFormat("client created for {0} -> {1}", settings, ClusterName);
        }

        [Pure]
        private static ConnectionSettings BuildConnectionSettings([NotNull] EsConnectionSettings settings)
        {
            var pool = new StaticConnectionPool(settings.Uris);
            var result = new ConnectionSettings(pool);

            // "SomeName" is "someName" in Elastic by default - make it "SomeName".
            result.DefaultFieldNameInferrer(f => f);

            result.PrettyJson(false);

            if (Trace)
            {
                m_log.Warn("DisableDirectStreaming is enabled and can affect performance");
                result.DisableDirectStreaming();
                result.OnRequestCompleted(h => m_log.DebugFormat("request: {0}", h.DebugInformation));
            }

            return result;
        }

        public bool IndexExists(string index)
        {
            index.IsCorrectEsIndexName(index);

            var r = Client.IndexExists(index);
            ThrowIfNotValid("IndexExists", index, r);
            return r.Exists;
        }

        public void DeleteIndex(string index)
        {
            index.IsCorrectEsIndexName(nameof(index));

            m_log.InfoFormat("deleting index {0}/{1}", ClusterName, index);

            var r = Client.DeleteIndex(index);
            if (r.Acknowledged)
            {
                m_log.Info($"index {ClusterName}/{index} has been deleted");
                return;
            }

            if (r.ServerError?.Error?.Type == "index_not_found_exception")
            {
                m_log.WarnFormat("index {0}/{1} doesn't exist", ClusterName, index);
                return;
            }

            ThrowIfNotValid("DeleteIndex", index, r);
        }

        public bool CreateIndex(
            EsIndexSettings index,
            Func<MappingsDescriptor, IPromise<IMappings>> func)
        {
            index.NotNull(nameof(index));
            func.NotNull(nameof(func));

            m_log.InfoFormat("creating index {0}/{1}", ClusterName, index.Name);

            var indexSettings = index.Settings != null
                ? new IndexSettings(index.Settings.ToDictionary(x => x.Key, x => (object)x.Value))
                : null;
            var indexState = new IndexState { Settings = indexSettings, };

            var r = Client.CreateIndex(
                index.Name,
                c => c
                    .InitializeUsing(indexState)
                    .Mappings(func));
            if (r.Acknowledged)
            {
                m_log.InfoFormat("index {0}/{1} has been created.", ClusterName, index.Name);
                return true;
            }

            if (r.ServerError?.Error?.Type == "resource_already_exists_exception")
            {
                m_log.WarnFormat("index {0}/{1} already exists.", ClusterName, index.Name);
                return false;
            }

            ThrowIfNotValid("CreateIndex", index.Name, r);
            return false;
        }

        public void Flush(string index)
        {
            index.IsCorrectEsIndexName(nameof(index));

            m_log.DebugFormat("flushing index {0}/{1}", ClusterName, index);

            var r = Client.Flush(index);
            ThrowIfNotValid("Flush", index, r);
        }

        public T Get<T>(string index, Id id)
            where T : class
        {
            index.IsCorrectEsIndexName(nameof(index));

            var r = Client.Get<T>(id, x => x.Index(index));
            if (r.ServerError?.Status == 404 && r.ServerError?.Error?.Type != "index_not_found_exception")
                return null;

            ThrowIfNotValid("Get id=" + ToString(id), index, r);

            return r.Source;
        }

        public async Task<StringResponse> Index(string index, string json)
        {
            index.IsCorrectEsIndexName(nameof(index));
            json.NotNullOrEmpty(nameof(json));

            var postData = PostData.String(json);

            var r = await Client.LowLevel.IndexAsync<StringResponse>(
                index,
                FieldConstants.PreferredTypeName,
                postData);
            if (r.Success) return r;

            throw new EsException($"Index json failed on {ClusterName}/{index}: {r.BuildErrorMessage()}");
        }

        public async Task CreateDocument<T>(string index, T doc)
            where T : class
        {
            var r = await Client.IndexAsync(doc, s => s.Index(index).OpType(OpType.Create));
            ThrowIfNotValid("Index", index, r);
        }

        public IBulkResponse IndexMany<T>(string index, IList<T> values)
            where T : class
        {
            values.NotNull(nameof(values));
            values.NotEmpty(nameof(values));
            index.IsCorrectEsIndexName(nameof(index));

            var r = Client.IndexMany(values, index);
            ThrowIfNotValid("IndexMany", index, r);

            return r;
        }

        public async Task<T> UpdateAsync<T>(
            string index,
            Id id,
            Action<IUpdateRequest<T, T>> selector)
            where T : class
        {
            index.NotNull(nameof(index));
            selector.NotNull(nameof(selector));

            var request = new UpdateRequest<T, T>(id, index)
                {
                    ScriptedUpsert = false,
                    SourceEnabled = true,
                    RetryOnConflict = UpdateRetryOnConflict,
                };
            selector(request);
            var r = await Client.UpdateAsync<T>(request);
            ThrowIfNotValid("Update id=" + ToString(id), index, r);

            var result = r.Get?.Source;
            if (result == null)
                throw new EsException($"Update returned null Source for id={ToString(id)} on {ClusterName}/{index}: {r.BuildErrorMessage()}");
            return result;
        }

        [Pure]
        public async Task<ISearchResponse<T>> SearchAsync<T>(
            string indexName,
            Func<SearchDescriptor<T>, ISearchRequest> selector)
            where T : class
        {
            indexName.IsCorrectEsIndexName(nameof(indexName));
            selector.NotNull(nameof(selector));

            var r = await Client.SearchAsync(selector);
            ThrowIfNotValid("Search", indexName, r);
            return r;
        }

        [Pure]
        public KeyValuePair<string, T> FetchFirstDocument<T>(
            string indexName,
            string entityType = FieldConstants.PreferredTypeName,
            Func<QueryContainerDescriptor<T>, QueryContainer> query = null,
            int timeoutMilliseconds = 20 * 1000)
            where T : class
        {
            indexName.IsCorrectEsIndexName(nameof(indexName));
            entityType.NotNullOrWhitespace(entityType);

            const int maxCount = 1;
            if (null == query)
            {
                const string operation = "Operation";
                query = q => q.Term(operation + FieldConstants.KeywordSuffix, entityType);
            }

            ISearchResponse<T> Select() => Client.Search<T>(
                s => s.Index(indexName)
                    .Type(FieldConstants.PreferredTypeName)
                    .From(0).Size(maxCount)
                    .Query(query));

            var result = FetchFirstDocument(
                Select,
                indexName,
                entityType,
                timeoutMilliseconds);
            return result;
        }

        [Pure]
        private KeyValuePair<string, T> FetchFirstDocument<T>(
            [NotNull] Func<ISearchResponse<T>> func,
            [NotNull] string indexName,
            [NotNull] string entityType,
            int timeoutMilliseconds)
            where T : class
        {
            timeoutMilliseconds.MustBePositive(nameof(timeoutMilliseconds));

            var stopwatch = Stopwatch.StartNew();

            for (;;)
            {
                var searchResponse = func();
                if (searchResponse.Documents.Count <= 0)
                {
                    if (!searchResponse.IsValid)
                        Debug.Fail(searchResponse.BuildErrorMessage());

                    if (stopwatch.ElapsedMilliseconds <= timeoutMilliseconds)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    CheckResponse(searchResponse, indexName);

                    throw new NotFoundElasticDocumentException(
                        $"The timeout ({timeoutMilliseconds} ms) has expired and the Elastic has not indexed the document.");
                }

                CheckResponse(searchResponse, indexName);

                var firstDocument = searchResponse.Documents.FirstOrDefault();
                if (null == firstDocument)
                    throw new Exception(
                        $"There must be '{entityType}' entity of type {typeof(T).FullName} in index='{indexName}'.");

                var hit = searchResponse.Hits?.FirstOrDefault();
                var id = hit?.Id;
                Debug.Assert(!String.IsNullOrEmpty(id));
                return new KeyValuePair<string, T>(id, firstDocument);
            }
        }

        private void CheckResponse(IResponse response, string index)
        {
            ThrowIfNotValid("Select from Elastic", index, response);
        }

        [Pure]
        public IGetMappingResponse GetMapping<T>(string index)
            where T : class
        {
            index.IsCorrectEsIndexName(nameof(index));

            var r = Client.GetMapping<T>(s => s.Index(index));
            ThrowIfNotValid("GetMapping", index, r);
            return r;
        }


        private void ThrowIfNotValid<T>(string callDetails, string index, T r) where T : IResponse
        {
            if (!r.IsValid)
                throw new EsException($"{callDetails} failed on {ClusterName}/{index}: {r.BuildErrorMessage()}");
        }

        private string ToString(Id id)
        {
            return (id as IUrlParameter).GetString(Client.ConnectionSettings);
        }
    }
}