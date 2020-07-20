using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.PageTracker.Storage;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Nest;
using GeoLocation = Com.O2Bionics.PageTracker.Contract.GeoLocation;

namespace Com.O2Bionics.PageTracker
{
    /// <summary>
    /// Elastic Search is used.
    /// </summary>
    public sealed class PageTrackerEs : BackgroundQueueProcessor<PageView>, IPageTracker
    {
        private readonly IGeoIpAddressResolver m_ipAddressResolver;
        private readonly IUserAgentParser m_userAgentParser;
        private readonly IEsClient m_elasticClient;
        private readonly IIdGenerator m_idGenerator;
        private readonly IFeatureServiceClient m_featureServiceClient;
        private readonly string m_indexName;

        public PageTrackerEs(
            [NotNull] IGeoIpAddressResolver ipAddressResolver,
            [NotNull] IUserAgentParser userAgentParser,
            [NotNull] PageTrackerSettings settings,
            [NotNull] IEsClient elasticClient,
            [NotNull] IIdGenerator idGenerator,
            [NotNull] IFeatureServiceClient featureServiceClient)
            : base(settings.NotNull(nameof(settings)).AddBufferSize, settings.AddBufferFlushTimeout)
        {
            m_ipAddressResolver = ipAddressResolver.NotNull(nameof(ipAddressResolver));
            m_userAgentParser = userAgentParser.NotNull(nameof(userAgentParser));
            m_elasticClient = elasticClient.NotNull(nameof(elasticClient));
            m_idGenerator = idGenerator.NotNull(nameof(idGenerator));
            m_featureServiceClient = featureServiceClient.NotNull(nameof(featureServiceClient));
            m_indexName = settings.PageVisitIndex.Name;
        }

        protected override void Save(List<PageView> buffer)
        {
            m_elasticClient.IndexMany(m_indexName, buffer);
        }

        public async Task<AddRecordResult> Add(DateTime timestampUtc, AddRecordArgs args)
        {
            Debug.Assert(0 < args.CustomerId);
            if (args.VisitorExternalId != null && args.VisitorExternalId.Length > PageTrackerConstants.VisitorExternalIdMaxLength)
            {
                Log.WarnFormat(
                    "VisitorExternalId truncated to maximum allowed length {0}: '{1}'",
                    PageTrackerConstants.VisitorExternalIdMaxLength,
                    args.VisitorExternalId);
                args.VisitorExternalId = args.VisitorExternalId.Substring(0, PageTrackerConstants.VisitorExternalIdMaxLength);
            }

            if (args.TimeZone == null)
                throw new ArgumentException("args.TimeZone can't be null");
            if (string.IsNullOrWhiteSpace(args.TimeZone.Description))
                throw new ArgumentException("args.TimeZone.Description can't be null or whitespace");
            if (args.TimeZone.Description.Length > PageTrackerConstants.TimeZoneDescriptionMaxLength)
            {
                Log.WarnFormat(
                    "TimeZone.Description truncated to maximum allowed length {0}: '{1}'",
                    PageTrackerConstants.TimeZoneDescriptionMaxLength,
                    args.TimeZone.Description);
                args.TimeZone.Description = args.TimeZone.Description.Substring(0, PageTrackerConstants.TimeZoneDescriptionMaxLength);
            }

            if (args.UserAgentString == null)
                args.UserAgentString = string.Empty;
            else if (args.UserAgentString.Length > PageTrackerConstants.UserAgentStringMaxLength)
            {
                Log.WarnFormat(
                    "UserAgentString truncated to maximum allowed length {0}: '{1}'",
                    PageTrackerConstants.UserAgentStringMaxLength,
                    args.UserAgentString);
                args.UserAgentString = args.UserAgentString.Substring(0, PageTrackerConstants.UserAgentStringMaxLength);
            }

            if (args.CustomText != null && args.CustomText.Length > PageTrackerConstants.CustomTextMaxLength)
            {
                Log.WarnFormat(
                    "CustomText truncated to maximum allowed length {0}: '{1}'",
                    PageTrackerConstants.CustomTextMaxLength,
                    args.CustomText);
                args.CustomText = args.CustomText.Substring(0, PageTrackerConstants.CustomTextMaxLength);
            }

            var userAgentInfo = string.IsNullOrEmpty(args.UserAgentString)
                ? null
                : m_userAgentParser.Parse(args.UserAgentString);

            GeoLocation location = null;
            var ip = args.Ip.ToString();
            if (await IsGeoLocationEnabled(args.CustomerId))
            {
                var isLocalHost = "127.0.0.1".Equals(ip, StringComparison.Ordinal);
                location = isLocalHost
                    ? new GeoLocation { City = "Minsk", Country = "Belarus", Point = new Point { lat = 53, lon = 28 } }
                    : m_ipAddressResolver.ResolveAddress(args.Ip);
            }

            var visitorId = args.VisitorId;
            if (visitorId == 0)
                visitorId = await m_idGenerator.NewId(IdScope.Visitor);

            var id = await m_idGenerator.NewId(IdScope.PageView);

            var pageView = new PageView
                {
                    Id = id.ToString("D"),
                    Timestamp = timestampUtc,
                    VisitorId = EsUnsignedHelper.ToEs(visitorId),
                    VisitorExternalId = args.VisitorExternalId,
                    CustomerId = args.CustomerId,
                    UriInfo = BuildUriInfo(args.Url),
                    UserAgent = userAgentInfo,
                    TimeZone = args.TimeZone,
                    CustomText = args.CustomText,
                    Location = location,
                    IpAddress = ip,
                };
            Add(pageView);

            var result = new AddRecordResult { VisitorId = visitorId, PageHistoryId = pageView.Id };
            return result;
        }

        [CanBeNull]
        private static string GetPart([CanBeNull] string value, char firstSymbol)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            if (firstSymbol != value[0])
                return value;

            var newValue = value.Substring(1);
            var result = string.IsNullOrEmpty(newValue) ? null : newValue;
            return result;
        }

        private static UriInfo BuildUriInfo([NotNull] Uri uri)
        {
            if (null == uri)
                throw new ArgumentNullException(nameof(uri));

            var link = uri.AbsoluteUri;
            if (string.IsNullOrEmpty(link))
                throw new ArgumentException("uri.AbsoluteUri must be not empty string");

            var result = new UriInfo
                {
                    Url = link,
                    Scheme = uri.Scheme,
                    Host = uri.Host,
                    Path = uri.LocalPath,
                    Port = uri.Port,
                    Query = GetPart(uri.Query, '?'),
                    Fragment = GetPart(uri.Fragment, '#'),
                };
            return result;
        }

        public async Task<GetHistoryResult> Get(
            uint customerId,
            ulong visitorId,
            int pageSize = PageTrackerConstants.PageHistoryGetLimit,
            SearchPositionInfo searchPosition = null)
        {
            customerId.MustBePositive(nameof(customerId));
            visitorId.MustBePositive(nameof(visitorId));
            pageSize.MustBePositive(nameof(pageSize));

            if (Log.IsDebugEnabled)
            {
                var whereClause = $"cid={customerId}, vid={visitorId}";

                if (null != searchPosition)
                    whereClause += $", sp={searchPosition}";

                Log.Debug($"{nameof(Get)} from Elastic {m_indexName}, {whereClause}.");
            }

            QueryContainer FilterFunc(QueryContainerDescriptor<PageView> q)
            {
                return q.Bool(
                    b => b.Must(
                        mu => mu.Term(x => x.CustomerId, customerId),
                        mu => mu.Term(x => x.VisitorId, EsUnsignedHelper.ToEs(visitorId))));
            }

            ISearchRequest Selector(SearchDescriptor<PageView> s)
            {
                if (null != searchPosition)
                    s.SearchAfter(searchPosition.AsObjectList());

                return s.Index(m_indexName)
                    .Type(FieldConstants.PreferredTypeName)
                    .From(0)
                    .Size(pageSize + 1)
                    .Query(FilterFunc)
                    .Sort(
                        d => d
                            .Field(x => x.Timestamp, SortOrder.Descending)
                            .Field(x => x.Id, SortOrder.Descending));
            }

            var response = await m_elasticClient.SearchAsync(
                m_indexName,
                (Func<SearchDescriptor<PageView>, ISearchRequest>)Selector);

            var result = new GetHistoryResult();
            if (null != response.Hits && 0 < response.Hits.Count)
            {
                var hasGeo = await IsGeoLocationEnabled(customerId);

                var hit = response.Hits.First().Source;
                result.Visitor = new PageHistoryVisitorInfo
                    {
                        Ip = IPAddress.Parse(hit.IpAddress),
                        TimestampUtc = hit.Timestamp,
                        VisitorExternalId = hit.VisitorExternalId,
                        TimeZone = hit.TimeZone,
                        UserAgent = hit.UserAgent,
                        IpLocation = hasGeo ? hit.Location : null
                    };

                result.Items = response.Hits
                    .Take(pageSize)
                    .Select(
                        d => new PageHistoryRecord
                            {
                                Id = d.Id,
                                TimestampUtc = d.Source.Timestamp,
                                CustomText = d.Source.CustomText,
                                Url = null == d.Source.UriInfo?.Url ? null : new Uri(d.Source.UriInfo.Url),
                            })
                    .ToList();

                result.HasMore = response.Hits.Count > pageSize;
                if (result.HasMore)
                    result.SearchPosition = new SearchPositionInfo(response.Hits.ElementAt(response.Hits.Count - 2).Sorts);
            }

            return result;
        }

        private async Task<bool> IsGeoLocationEnabled(uint customerId)
        {
            var value = await m_featureServiceClient.GetBool(customerId, FeatureCodes.IsGeoLocationEnabled);
            return value;
        }
    }
}