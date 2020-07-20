using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Com.O2Bionics.PageTracker.DataModel;
using log4net;
using MySql.Data.MySqlClient;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.PageTracker
{
    /// <summary>
    /// My SQL is used.
    /// </summary>
    public sealed class PageTrackerMySql : IPageTracker
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(PageTrackerMySql));

        private readonly IGeoIpAddressResolver m_ipAddressResolver;
        private readonly IUserAgentParser m_userAgentParser;
        private readonly IDatabaseFactory m_databaseFactory;
        private readonly IFeatureServiceClient m_featureServiceClient;

        public PageTrackerMySql(
            IGeoIpAddressResolver ipAddressResolver,
            IUserAgentParser userAgentParser,
            IDatabaseFactory databaseFactory,
            IFeatureServiceClient featureServiceClient)
        {
            m_ipAddressResolver = ipAddressResolver;
            m_userAgentParser = userAgentParser;
            m_databaseFactory = databaseFactory;
            m_featureServiceClient = featureServiceClient;
        }

        int IPageTracker.QueueSize => 0;

        public void Flush()
        {
        }

        //TODO: task-393 is not implemented for My SQL.
        void IDisposable.Dispose()
        {
        }

        public Task<AddRecordResult> Add(DateTime timestampUtc, AddRecordArgs args)
        {
            if (args.VisitorExternalId != null && args.VisitorExternalId.Length > PageTrackerConstants.VisitorExternalIdMaxLength)
            {
                m_log.WarnFormat(
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
                m_log.WarnFormat(
                    "TimeZone.Description truncated to maximum allowed length {0}: '{1}'",
                    PageTrackerConstants.TimeZoneDescriptionMaxLength,
                    args.TimeZone.Description);
                args.TimeZone = new TimeZoneDescription(
                    args.TimeZone.Offset,
                    args.TimeZone.Description.Substring(0, PageTrackerConstants.TimeZoneDescriptionMaxLength));
            }

            if (args.UserAgentString == null) args.UserAgentString = "";
            if (args.UserAgentString.Length > PageTrackerConstants.UserAgentStringMaxLength)
            {
                m_log.WarnFormat(
                    "UserAgentString truncated to maximum allowed length {0}: '{1}'",
                    PageTrackerConstants.UserAgentStringMaxLength,
                    args.UserAgentString);
                args.UserAgentString = args.UserAgentString.Substring(0, PageTrackerConstants.UserAgentStringMaxLength);
            }

            if (args.CustomText != null && args.CustomText.Length > PageTrackerConstants.CustomTextMaxLength)
            {
                m_log.WarnFormat(
                    "CustomText truncated to maximum allowed length {0}: '{1}'",
                    PageTrackerConstants.CustomTextMaxLength,
                    args.CustomText);
                args.CustomText = args.CustomText.Substring(0, PageTrackerConstants.CustomTextMaxLength);
            }

            var userAgentInfo = m_userAgentParser.Parse(args.UserAgentString);

            var addressBytes = args.Ip.GetAddressBytes();
            GeoLocation location = null;

            var isGeoLocationEnabled = IsGeoLocationEnabled(args.CustomerId);

            for (var repeat = 0; repeat < 15; repeat++)
            {
                try
                {
                    var result = m_databaseFactory.Query(
                        db =>
                            {
                                var ipAddress = GetIpAddress(db, addressBytes);
                                var ipAddressId = ipAddress.Item1;
                                var isIpAddressResolved = ipAddress.Item2;

                                var updateLocationFlag = false;
                                if (isGeoLocationEnabled && !isIpAddressResolved)
                                {
                                    location = args.Ip.ToString() != "127.0.0.1"
                                        ? m_ipAddressResolver.ResolveAddress(args.Ip)
                                        : new GeoLocation
                                            {
                                                City = "Minsk",
                                                Country = "Belarus",
                                                Point = new Point { lat = 53, lon = 28 }
                                            };
                                    //m_ipAddressResolver.ResolveAddress(IPAddress.Parse("121.23.21.1"));
                                    updateLocationFlag = true;
                                }

                                var result1 = AddRecord(
                                    db,
                                    timestampUtc,
                                    ipAddressId,
                                    addressBytes,
                                    location,
                                    args.CustomerId,
                                    (uint)args.VisitorId,
                                    args.VisitorExternalId,
                                    args.TimeZone,
                                    userAgentInfo,
                                    args.Url,
                                    args.CustomText,
                                    updateLocationFlag);

                                return result1;
                            });
                    return Task.FromResult(result);
                }
                catch (MySqlException e) when (e.Number == 1205 || e.Number == 1213)
                {
                    m_log.WarnFormat("deadlock detected, attempt {0}", repeat);
                }
                catch (MySqlException e) when (e.Number == 1062)
                {
                    m_log.WarnFormat("duplicate key exception, attempt {0}", repeat);
                }
                catch (MySqlException e)
                {
                    m_log.WarnFormat("attempt {0}, {1} ex {2}", repeat, e.Number, e);
                    throw;
                }
            }

            throw new Exception("Operation failed");
        }

        private Tuple<uint?, bool> GetIpAddress(Database db, byte[] addressBytes)
        {
            const string sql = "select id, is_resolved from ip_address where ip = @ip";
            using (var cmd = new MySqlCommand(sql))
            {
                cmd.Parameters.Add("ip", MySqlDbType.VarBinary).Value = addressBytes;
                using (var reader = db.ExecuteReader(cmd))
                {
                    var hasData = reader.Read();
                    return Tuple.Create(hasData ? reader.GetUInt32(0) : (uint?)null, hasData && reader.GetBoolean(1));
                }
            }
        }


        private static AddRecordResult AddRecord(
            Database db,
            DateTime timestampUtc,
            uint? ipAddressId,
            byte[] ipAddressBytes,
            GeoLocation location,
            uint customerId,
            uint visitorId,
            string visitorExternalId,
            TimeZoneDescription timeZone,
            UserAgentInfo userAgentInfo,
            Uri uri,
            string customText,
            bool updateIpLocationFlag)
        {
            using (var cmd = new MySqlCommand("add_record"))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                AddParam(cmd, "timestampUtc", MySqlDbType.DateTime, timestampUtc);
                AddParam(cmd, "ipAddressId", MySqlDbType.UInt32, ipAddressId ?? (object)DBNull.Value);
                AddParam(cmd, "ip", MySqlDbType.VarBinary, !ipAddressId.HasValue ? ipAddressBytes : (object)DBNull.Value);
                var hasLocationInfo = /*!ipAddressId.HasValue &&*/ location != null;
                AddParam(cmd, "ipCountryName", MySqlDbType.VarChar, hasLocationInfo ? location.Country : (object)DBNull.Value);
                AddParam(cmd, "ipCityName", MySqlDbType.VarChar, hasLocationInfo ? location.City : (object)DBNull.Value);
                AddParam(cmd, "ipLatitude", MySqlDbType.Int32, hasLocationInfo ? (int?)location.Point?.lat : (object)DBNull.Value);
                AddParam(cmd, "ipLongitude", MySqlDbType.Int32, hasLocationInfo ? (int?)location.Point?.lon : (object)DBNull.Value);
                AddParam(cmd, "customerId", MySqlDbType.UInt32, customerId);
                AddParam(cmd, "visitorId", MySqlDbType.UInt32, visitorId);
                AddParam(cmd, "visitorExternalId", MySqlDbType.VarChar, visitorExternalId ?? (object)DBNull.Value);
                AddParam(cmd, "timezoneOffset", MySqlDbType.Int32, timeZone.Offset);
                AddParam(cmd, "timezoneDescription", MySqlDbType.VarChar, timeZone.Description);
                AddParam(cmd, "userAgentText", MySqlDbType.VarChar, userAgentInfo.UserAgentString);
                AddParam(cmd, "userAgentDevice", MySqlDbType.VarChar, userAgentInfo.Device);
                AddParam(cmd, "userAgentOs", MySqlDbType.VarChar, userAgentInfo.Os);
                AddParam(cmd, "userAgentAgent", MySqlDbType.VarChar, userAgentInfo.UserAgent);

                // scheme://host[:port][/path][?query][#fragment]
                AddParam(cmd, "siteScheme", MySqlDbType.VarChar, uri.Scheme);
                AddParam(cmd, "siteHost", MySqlDbType.VarChar, uri.Host);
                AddParam(cmd, "sitePort", MySqlDbType.Int32, uri.Port);
                AddParam(cmd, "urlPath", MySqlDbType.VarChar, uri.LocalPath);

                var query = uri.Query;
                if (!string.IsNullOrEmpty(query) && query.StartsWith("?")) query = query.Substring(1);
                AddParam(cmd, "urlQuery", MySqlDbType.VarChar, query);

                var fragment = uri.Fragment;
                if (!string.IsNullOrEmpty(fragment) && fragment.StartsWith("#")) fragment = fragment.Substring(1);
                AddParam(cmd, "urlFragment", MySqlDbType.VarChar, fragment);

                AddParam(cmd, "customText", MySqlDbType.VarChar, customText);

                AddParam(cmd, "updateIpLocationFlag", MySqlDbType.Bit, updateIpLocationFlag);

                cmd.Parameters.Add("pageHistoryId", MySqlDbType.UInt32).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("visitorId2", MySqlDbType.UInt32).Direction = ParameterDirection.Output;

                db.Execute(cmd);

                return new AddRecordResult
                    {
                        PageHistoryId = Convert.ToUInt32(cmd.Parameters["pageHistoryId"].Value).ToString(CultureInfo.InvariantCulture),
                        VisitorId = Convert.ToUInt32(cmd.Parameters["visitorId2"].Value),
                    };
            }
        }

        private static void AddParam<T>(MySqlCommand cmd, string name, MySqlDbType type, T value)
        {
            var p = cmd.Parameters.Add(name, type);
            p.Direction = ParameterDirection.Input;
            p.Value = value;
        }

        public Task<GetHistoryResult> Get(uint customerId, ulong visitorIdRaw, int pageSize, SearchPositionInfo searchPosition = null)
        {
            var visitorId = (uint)visitorIdRaw;
            uint pageHistoryId = 0;
            var isGeoLocationEnabled = IsGeoLocationEnabled(customerId);

            var result = m_databaseFactory.Query(
                db =>
                    {
                        var visitorInfo = GetPageHistoryVisitorInfo(db, visitorId, pageHistoryId, isGeoLocationEnabled);
                        if (visitorInfo == null)
                            return new GetHistoryResult { Items = new List<PageHistoryRecord>() };

                        var items = GetPageHistoryRecords(db, visitorId, pageHistoryId, pageSize);

                        return new GetHistoryResult
                            {
                                Visitor = visitorInfo,
                                Items = items
                            };
                    });
            return Task.FromResult(result);
        }

        private static List<PageHistoryRecord> GetPageHistoryRecords(Database db, uint visitorId, uint pageHistoryId, int pageSize)
        {
            var filter = "v.id = @visitorId";
            if (pageHistoryId > 0)
                filter += " and ph.id < @pageHistoryId";

            var sql = string.Format(
                @"
                    select
                        ph.id,
                        ph.timestamp,
                        s.scheme,
                        s.host,
                        s.port,
                        ph.path,
                        ph.query,
                        ph.fragment,
                        ph.custom_text
                    from 
                        page_history ph
                        inner join visitor v on ph.visitor_id = v.id
                        inner join site s on ph.site_id = s.id
                    where {0}
                    order by 
                        ph.id desc
                    limit {1}",
                filter,
                pageSize);

            using (var cmd = new MySqlCommand(sql))
            {
                cmd.Parameters.Add("visitorId", MySqlDbType.UInt32).Value = visitorId;
                if (pageHistoryId > 0)
                    cmd.Parameters.Add("pageHistoryId", MySqlDbType.UInt32).Value = pageHistoryId;

                using (var reader = db.ExecuteReader(cmd))
                {
                    var items = new List<PageHistoryRecord>();
                    while (reader.Read())
                    {
                        var r = new PageHistoryRecord
                            {
                                Id = reader.GetUInt32(0).ToString(CultureInfo.InvariantCulture),
                                TimestampUtc = reader.GetDateTime(1),
                                Url = CreateUri(
                                    reader.GetString(2),
                                    reader.GetString(3),
                                    reader.GetInt32(4),
                                    reader.GetString(5),
                                    reader.GetString(6),
                                    reader.GetString(7)),
                                CustomText = reader.IsDBNull(8) ? null : reader.GetString(8),
                            };
                        items.Add(r);
                    }

                    return items;
                }
            }
        }


        private const string PageHistoryVisitorInfoSql = @"
                    select
                        ph.id,
                        v.external_id,
                        v.timestamp,
                        a.ip,
                        tz.offset,
                        tz.description,
                        ua.device,
                        ua.os,
                        ua.agent,
                        ua.text{1}
                    from 
                        page_history ph
                        inner join visitor v on ph.visitor_id = v.id
                        inner join ip_address a on ph.ip_address_id = a.id
                        inner join timezone tz on ph.timezone_id = tz.id
                        inner join user_agent ua on ph.user_agent_id = ua.id
                        {2}
                    where {0}
                    order by  ph.id desc
                    limit 1";

        private const string PageHistoryVisitorInfoSqlGeoLocationFields = @"
                        ,
                        country.name,
                        city.name,
                        a.latitude,
                        a.longitude";

        private const string PageHistoryVisitorInfoSqlGeoLocationJoin = @"
                        left join ip_country country on a.country_id = country.id
                        left join ip_city city on a.city_id = city.id                        ";

        private static PageHistoryVisitorInfo GetPageHistoryVisitorInfo(
            Database db,
            uint visitorId,
            uint pageHistoryId,
            bool isGeoLocationEnabled)
        {
            var filter = "v.id = @visitorId";
            if (pageHistoryId > 0)
                filter += " and ph.id < @pageHistoryId";

            var sql = string.Format(
                PageHistoryVisitorInfoSql,
                filter,
                isGeoLocationEnabled ? PageHistoryVisitorInfoSqlGeoLocationFields : "",
                isGeoLocationEnabled ? PageHistoryVisitorInfoSqlGeoLocationJoin : "");

            using (var cmd = new MySqlCommand(sql))
            {
                cmd.Parameters.Add("visitorId", MySqlDbType.VarBinary).Value = visitorId;
                if (pageHistoryId > 0)
                    cmd.Parameters.Add("pageHistoryId", MySqlDbType.UInt32).Value = pageHistoryId;

                using (var reader = db.ExecuteReader(cmd))
                {
                    if (!reader.Read()) return null;

                    return new PageHistoryVisitorInfo
                        {
                            VisitorExternalId = reader.IsDBNull(1) ? null : reader.GetString(1),
                            TimestampUtc = reader.GetDateTime(2),
                            Ip = new IPAddress(GetAddressBytes(reader, 3)),
                            TimeZone = new TimeZoneDescription(reader.GetInt32(4), reader.GetString(5)),
                            UserAgent = new UserAgentInfo
                                {
                                    Device = reader.GetString(6),
                                    Os = reader.GetString(7),
                                    UserAgent = reader.GetString(8),
                                    UserAgentString = reader.GetString(9),
                                },
                            IpLocation = isGeoLocationEnabled
                                ? ReadPageHistoryVisitorGeoLocation(reader)
                                : null,
                        };
                }
            }
        }

        private static GeoLocation ReadPageHistoryVisitorGeoLocation(MySqlDataReader reader)
        {
            var info = new GeoLocation
                {
                    Country = reader.IsDBNull(10) ? null : reader.GetString(10),
                    City = reader.IsDBNull(11) ? null : reader.GetString(11),
                };

            var latitude = reader.IsDBNull(12) ? null : (int?)reader.GetInt32(12);
            var longitude = reader.IsDBNull(13) ? null : (int?)reader.GetInt32(13);
            if (latitude.HasValue || longitude.HasValue)
            {
                info.Point = new Point();
                if (latitude.HasValue)
                    info.Point.lat = latitude.Value;
                if (longitude.HasValue)
                    info.Point.lon = longitude.Value;
            }

            return info.Country == null && info.City == null && info.Point == null ? null : info;
        }

        private static byte[] GetAddressBytes(MySqlDataReader reader, int i)
        {
            var buf = new byte[16];
            var count = reader.GetBytes(i, 0, buf, 0, 16);
            var result = new byte[count];
            Array.Copy(buf, result, count);
            return result;
        }

        private bool IsGeoLocationEnabled(uint customerId)
        {
            var value = m_featureServiceClient.GetBool(customerId, FeatureCodes.IsGeoLocationEnabled).WaitAndUnwrapException();
            return value;
        }

        private static Uri CreateUri(string scheme, string host, int port, string path, string query, string fragment)
        {
            var uri = new UriBuilder(scheme, host, (scheme == "http" && port == 80) || (scheme == "https" && port == 443) ? -1 : port);
            if (!string.IsNullOrEmpty(path)) uri.Path = path;
            if (!string.IsNullOrEmpty(query)) uri.Query = query;
            if (!string.IsNullOrEmpty(fragment)) uri.Fragment = fragment;
            return uri.Uri;
        }
    }
}