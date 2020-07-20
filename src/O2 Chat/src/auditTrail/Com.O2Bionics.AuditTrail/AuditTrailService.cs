using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Contract.Names;
using Com.O2Bionics.AuditTrail.Contract.Settings;
using Com.O2Bionics.AuditTrail.Properties;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using JetBrains.Annotations;
using log4net;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Com.O2Bionics.AuditTrail
{
    /// <summary>
    ///     Save to/read from the Elastic server.
    /// </summary>
    public sealed class AuditTrailService : IAuditTrailService
    {
        private const string OperationKey = "a", StatusKey = "b", AuthorKey = "c", AuthorTopKey = "d";

        private static readonly ILog m_log = LogManager.GetLogger(typeof(AuditTrailService));
        private static readonly JsonConverter[] m_jsonConverters = new JsonConverter[0];

        private readonly string m_indexNameBase;
        private readonly IEsClient m_elasticClient;
        private readonly HashSet<string> m_productCodes;

        public AuditTrailService([NotNull] AuditTrailServiceSettings settings, [NotNull] IEsClient elasticClient)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (settings.ProductCodes.Count == 0)
                throw new Exception(Resources.EmptyProductCodesError);
            m_elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));

            m_indexNameBase = settings.Index.Name;
            m_productCodes = settings.ProductCodes.ToHashSet();
        }

        public async Task Save(string productCode, string serializedJson)
        {
            if (string.IsNullOrEmpty(productCode))
                throw new ArgumentNullException(nameof(productCode));
            if (string.IsNullOrEmpty(serializedJson))
                throw new ArgumentNullException(nameof(serializedJson));
            if (!m_productCodes.Contains(productCode))
                throw new ArgumentException(string.Format(Resources.UnknownProductCode1, productCode.LimitLength()));

            var indexName = IndexNameFormatter.Format(m_indexNameBase, productCode);
            if (m_log.IsDebugEnabled)
                m_log.Debug($"Save to Elastic {indexName} '{serializedJson}'.");

            await m_elasticClient.Index(indexName, serializedJson);
        }

        public async Task<FacetResponse> SelectFacets(Filter filter)
        {
            if (!m_productCodes.Contains(filter.ProductCode))
                throw new ArgumentException(string.Format(Resources.UnknownProductCode1, filter.ProductCode.LimitLength()));

            if (!string.IsNullOrEmpty(filter.Substring))
                filter.Substring = filter.Substring.Trim();

            var indexName = IndexNameFormatter.Format(m_indexNameBase, filter.ProductCode);
            if (m_log.IsDebugEnabled)
                m_log.Debug($"{nameof(SelectFacets)} from Elastic {indexName} '{filter}'.");

            const int maxSizeId = 10 * 1000;

            QueryContainer FilterFunc(QueryContainerDescriptor<object> q)
            {
                return q.Bool(
                    b => b.Must(
                        mu => string.IsNullOrEmpty(filter.CustomerId) ? mu : mu.Term(ServiceConstants.CustomerId, filter.CustomerId),
                        mu => null == filter.Operations || 0 == filter.Operations.Count
                            ? mu
                            : mu.Terms(
                                d => d.Field(FieldNames.OperationPropertyNameKeyword)
                                    .Terms(filter.Operations)),
                        mu => null == filter.Statuses || 0 == filter.Statuses.Count
                            ? mu
                            : mu.Terms(
                                d => d.Field(FieldNames.StatusPropertyNameKeyword)
                                    .Terms(filter.Statuses)),
                        mu => null == filter.AuthorIds || 0 == filter.AuthorIds.Count
                            ? mu
                            : mu.Terms(z => z.Field(FieldNames.AuthorIdKeyword).Terms(filter.AuthorIds)),
                        mu => filter.FromTime == filter.ToTime
                            ? mu
                            : mu.DateRange(
                                f => f.Field(FieldNames.TimeStampPropertyName)
                                    .GreaterThanOrEquals(filter.FromTime)
                                    .LessThan(filter.ToTime)),
                        mu => string.IsNullOrEmpty(filter.Substring)
                            ? mu
                            : mu.MultiMatch(
                                p => p.Type(TextQueryType.PhrasePrefix)
                                    .Fields(
                                        fi => fi.Fields(
                                            filter.ChangedOnly ? FieldNames.ChangedPropertyName : FieldNames.AllPropertyName))
                                    .Query(filter.Substring))));
            }

            ISearchRequest Selector(SearchDescriptor<object> s)
            {
                if (null != filter.SearchPosition)
                    s.SearchAfter(filter.SearchPosition.AsObjectList());

                return s.Index(indexName)
                    .Type(FieldConstants.PreferredTypeName)
                    .From(filter.FromRow)
                    .Size(filter.PageSize)
                    .Query(FilterFunc)
                    .Source(
                        so => so.IncludeAll()
                            .Excludes(excl => excl.Fields(FieldNames.AllPropertyName, FieldNames.ChangedPropertyName)))
                    .Sort(
                        d => d.Field(FieldNames.TimeStampPropertyName, SortOrder.Descending)
                            .Field(FieldNames.IdPropertyName, SortOrder.Descending))
                    .Aggregations(
                        a => a.Terms(
                                OperationKey,
                                tty => tty.Field(FieldNames.OperationPropertyNameKeyword).Size(maxSizeId))
                            .Terms(
                                StatusKey,
                                tty => tty.Field(FieldNames.StatusPropertyNameKeyword).Size(maxSizeId))
                            .Terms(
                                AuthorKey,
                                tty => tty.Field(FieldNames.AuthorIdKeyword)
                                    .Size(maxSizeId)
                                    .Aggregations(
                                        a2 => a2.TopHits(
                                            AuthorTopKey,
                                            h => h.Sort(
                                                    so1 => so1.Field(FieldNames.TimeStampPropertyName, SortOrder.Descending))
                                                .From(0)
                                                .Size(1)))));
            }

            var searchResponse = await m_elasticClient.SearchAsync(
                indexName,
                (Func<SearchDescriptor<object>, ISearchRequest>)Selector);
            var result = BuildResponse(searchResponse);
            return result;
        }

        [CanBeNull]
        private static IEnumerable<KeyedBucket<object>> GetBuckets(BucketAggregate bucketAggregate)
        {
            if (null == bucketAggregate || 0 == bucketAggregate.Items.Count)
                return null;

            var result = bucketAggregate.Items.OfType<KeyedBucket<object>>();
            return result;
        }

        [CanBeNull]
        private static IEnumerable<Facet> PlainFacets(BucketAggregate bucketAggregate)
        {
            var buckets = GetBuckets(bucketAggregate);
            var result = buckets?.Select(bucket => new { id = bucket.Key?.ToString(), count = bucket.DocCount ?? 0L })
                .Where(an => !string.IsNullOrEmpty(an.id))
                .Select(an => new Facet(an.id, null, an.count));
            return result;
        }

        private static List<Facet> FacetOperation(BucketAggregate bucketAggregate)
        {
            var facets = PlainFacets(bucketAggregate);
            var result = facets?.ToList();
            return null != result && 0 < result.Count ? result : null;
        }

        private static List<Facet> FacetStatus(BucketAggregate bucketAggregate)
        {
            var facets = PlainFacets(bucketAggregate);
            var result = facets?.Where(item => !string.IsNullOrEmpty(item.Id)).ToList();
            return null != result && 0 < result.Count ? result : null;
        }

        private static List<Facet> FacetAuthor(BucketAggregate bucketAggregate)
        {
            var buckets = GetBuckets(bucketAggregate);
            var result = buckets?
                .Select(b => b.TryGetValue(AuthorTopKey, out var aggregate) ? aggregate : null)
                .Where(c => null != c).OfType<TopHitsAggregate>()
                .Select(
                    d => new
                        {
                            c = d.Total,
                            history = d.Hits<AuditEventShort>().FirstOrDefault(h => !string.IsNullOrEmpty(h?.Source?.Author?.Id))
                        })
                .Where(e => null != e.history)
                .Select(e => new Facet(e.history.Source.Author.Id, e.history.Source.Author.Name, e.c))
                .ToList();
            return null != result && 0 < result.Count ? result : null;
        }

        [CanBeNull]
        private static List<string> GetDocuments(ISearchResponse<object> searchResponse)
        {
            var documents = searchResponse.Documents;
            if (null == documents || 0 == documents.Count)
                return null;

            var result = new List<string>(documents.Count);
            foreach (var document in documents)
            {
                var str = DocumentToString(document);
                if (!string.IsNullOrEmpty(str))
                    result.Add(str);
            }

            return 0 < result.Count ? result : null;
        }

        private static string DocumentToString(object document)
        {
            string result;
            if (document is JToken token)
                //Disable the pretty printout.
                result = token.ToString(Formatting.None, m_jsonConverters);
            else
                result = document?.ToString();
            return result;
        }

        private static FacetResponse BuildResponse(ISearchResponse<object> searchResponse)
        {
            var result = new FacetResponse
                {
                    RawDocuments = GetDocuments(searchResponse)
                };
            if (null != searchResponse.Aggregations && 0 < searchResponse.Aggregations.Count)
            {
                if (searchResponse.Aggregations.TryGetValue(OperationKey, out var oper))
                    result.Operations = FacetOperation(oper as BucketAggregate);

                if (searchResponse.Aggregations.TryGetValue(StatusKey, out var statuses))
                    result.Statuses = FacetStatus(statuses as BucketAggregate);

                if (searchResponse.Aggregations.TryGetValue(AuthorKey, out var authors))
                    result.Authors = FacetAuthor(authors as BucketAggregate);
            }

            return 0 < result.Count ? result : null;
        }
    }
}