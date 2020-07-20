using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.FeatureService.Impl.DataModel;
using log4net;
using Oracle.ManagedDataAccess.Client;

namespace Com.O2Bionics.FeatureService.Impl
{
    public class FeaturesManager : IFeaturesManager
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(FeaturesManager));

        private readonly FeatureServiceSettings m_settings;
        private readonly IDatabaseFactory m_databaseFactory;
        private readonly IFeatureServiceCacheManager m_cacheManager;

        public FeaturesManager(
            FeatureServiceSettings settings,
            IDatabaseFactory databaseFactory,
            IFeatureServiceCacheManager cacheManager)
        {
            m_settings = settings;
            m_databaseFactory = databaseFactory;
            m_cacheManager = cacheManager;
        }

        private class FeatureValue
        {
            public FeatureValue(string featureCode, string cacheKey)
            {
                FeatureCode = featureCode;
                CacheKey = cacheKey;
            }

            public readonly string FeatureCode;
            public readonly string CacheKey;
            public string Value;

            public bool HasValue
            {
                get { return Value != null; }
            }
        }

        public Dictionary<string, string> GetFeatureValue(
            string productCode,
            int userId,
            HashSet<string> featureCodes,
            bool? ignoreCache = null,
            int? timeToLiveSeconds = null,
            bool? logProcessing = null,
            bool? logSqlQuery = null)
        {
            if (string.IsNullOrWhiteSpace(productCode))
                throw new ArgumentException("Can't be null or whitespace", "productCode");

            if (userId <= 0)
                throw new ArgumentException("Can't be less or equal to 0, but is " + userId, "userId");

            if (featureCodes == null)
                throw new ArgumentNullException("featureCodes");
            if (!featureCodes.Any())
                throw new ArgumentException("Can't be empty", "featureCodes");
            if (featureCodes.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Can't contain null or whitespace", "featureCodes");

            var effectiveLogProcessing = logProcessing ?? m_settings.LogProcessing;
            var effectiveLogSqlQuery = logSqlQuery ?? m_databaseFactory.LogEnabled;

            IDisposable context = null;
            if (effectiveLogProcessing || effectiveLogSqlQuery)
                context = ThreadContext.Stacks["NDC"].Push(Guid.NewGuid().ToString("N").Substring(16));
            try
            {
                if (effectiveLogProcessing && m_log.IsDebugEnabled)
                    m_log.DebugFormat(
                        "GetFeatureValue(pc='{0}', uid={1}, fc=[{2}], ic={3}, ttl={4}, logp={5}, logq={6},)",
                        productCode,
                        userId,
                        string.Join(", ", featureCodes.Select(x => "'" + x + "'")),
                        ignoreCache,
                        timeToLiveSeconds,
                        logProcessing,
                        logSqlQuery);


                var resultValues = featureCodes
                    .Select(
                        featureCode => new FeatureValue(
                            featureCode,
                            m_cacheManager == null ? null : m_cacheManager.GetKey(productCode, userId, featureCode)))
                    .ToList();

                var effectiveIgnoreCache = ignoreCache ?? false;
                if (m_cacheManager != null && !effectiveIgnoreCache)
                {
                    foreach (var fv in resultValues)
                        fv.Value = m_cacheManager.GetValue(fv.CacheKey);
                }

                GetUnresolvedValuesFromDatabase(productCode, userId, resultValues, timeToLiveSeconds, logSqlQuery, effectiveLogProcessing);

                return resultValues.ToDictionary(x => x.FeatureCode, x => x.Value, StringComparer.InvariantCultureIgnoreCase);
            }
            finally
            {
                if (context != null) context.Dispose();
            }
        }

        public void ResetCache()
        {
            m_log.Info(nameof(ResetCache));

            if (m_cacheManager == null)
            {
                m_log.Info("ResetCache: cacheManager was not configured. ignoring command.");
                return;
            }

            m_cacheManager.ResetCache();
            m_log.Info("ResetCache: done.");
        }

        public Dictionary<string, string> Ping()
        {
            m_log.Debug(nameof(Ping));

            var result = new Dictionary<string, string>();
            foreach (var productCode in m_databaseFactory.ProductCodes)
            {
                string status;
                try
                {
                    m_databaseFactory.Query(productCode, db => { db.ExecuteScalar("select 1 from dual"); });
                    status = "db ok";
                }
                catch (Exception e)
                {
                    m_log.Error($"Ping {productCode}", e);
                    status = e.Message;
                }
                result.Add(productCode, status);
            }
            return result;
        }


        private void GetUnresolvedValuesFromDatabase(
            string productCode,
            int userId,
            List<FeatureValue> resultValues,
            int? timeToLiveSeconds,
            bool? logSqlQuery,
            bool logProcessing)
        {
            if (logProcessing && m_log.IsDebugEnabled)
            {
                var resolved = resultValues.Where(x => x.HasValue).Select(x => x.FeatureCode).ToList();
                if (resolved.Any())
                    m_log.DebugFormat("cache hit: uid={0} fc=[{1}]", userId, string.Join(",", resolved));
            }

            var unresolvedResultValues = resultValues.Where(x => !x.HasValue).ToList();
            if (unresolvedResultValues.Any())
            {
                if (logProcessing && m_log.IsDebugEnabled)
                    m_log.DebugFormat("cache miss: uid={0} fc=[{1}]", userId, string.Join(",", unresolvedResultValues.Select(x => x.FeatureCode)));

                var values = m_databaseFactory.Query(
                    productCode,
                    db =>
                        {
                            var featureCodeInfo = FeatureInfo.Get(
                                db,
                                unresolvedResultValues.Select(x => x.FeatureCode).ToList(),
                                logProcessing);

                            GetCustomerOverrideValues(
                                db,
                                userId,
                                featureCodeInfo,
                                logProcessing);

                            var notDefinedFeatures = featureCodeInfo
                                .Where(x => x.Value.Value == null)
                                .ToDictionary(x => x.Key, x => x.Value);
                            GetSubscriptionsValue(db, userId, notDefinedFeatures, logProcessing);

                            return featureCodeInfo.Values.ToDictionary(x => x.Code, x => x.Value, StringComparer.InvariantCultureIgnoreCase);
                        },
                    logSqlQuery);

                foreach (var fv in unresolvedResultValues)
                {
                    string value;
                    if (values.TryGetValue(fv.FeatureCode, out value))
                        fv.Value = value;
                }

                if (m_cacheManager != null)
                    foreach (var fv in unresolvedResultValues.Where(x => x.HasValue))
                        m_cacheManager.SetValue(fv.CacheKey, fv.Value, timeToLiveSeconds);
            }
        }

        private class SubscriptionRecord
        {
            public int FeatureId { get; private set; }
            public string Value { get; private set; }
            public int SubscriptionSkey { get; private set; }
            public int? Quantity { get; private set; }

            private int ServiceId { get; set; }
            private string ServiceName { get; set; }
            private int? ServiceTypeId { get; set; }

            public bool IsAddon
            {
                get { return ServiceTypeId != 0; }
            }

            private SubscriptionRecord(IDataReader reader)
            {
                FeatureId = reader.GetInt32(0);
                Value = reader.GetString(1);
                ServiceId = reader.GetInt32(2);
                ServiceName = reader.GetString(3) ?? "";
                ServiceTypeId = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4);
                SubscriptionSkey = reader.GetInt32(5);
                Quantity = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6);
            }

            private const string SqlServiceSubscriptionValues =
                @"  select
                        sf.feature_id,
                        sf.feature_value,
                        s.SERVICE_ID,
                        s.SERVICE_NAME,
                        s.SERVICE_TYPE_ID,
                        ss.SUBSCRIPTION_SKEY,
                        ss.QUANTITY
                    from
                        service_features sf
                            inner join services s on s.service_id = sf.service_id
                            inner join service_subscription ss on ss.service_id = sf.service_id
                    where
                        ss.userid = :userId
                        and sf.feature_id in ({0})
                        and nvl(ss.is_deleted, 0) = 0";

            public static List<SubscriptionRecord> Get(Database db, int userId, Dictionary<int, FeatureInfo> featureInfo)
            {
                if (!featureInfo.Any())
                    return new List<SubscriptionRecord>();

                var sql = string.Format(
                    SqlServiceSubscriptionValues,
                    string.Join(",", Enumerable.Range(0, featureInfo.Count).Select(i => ":p" + i)));
                using (var cmd = new OracleCommand(sql))
                {
                    cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

                    var i = 0;
                    foreach (var featureId in featureInfo.Keys)
                        cmd.Parameters.Add("p" + i++, OracleDbType.Int32).Value = featureId;

                    var records = new List<SubscriptionRecord>();
                    using (var reader = db.ExecuteReader(cmd))
                    {
                        while (reader.Read())
                        {
                            var record = new SubscriptionRecord(reader);
                            if (record.Value != null) records.Add(record);
                        }
                    }
                    return records;
                }
            }

            public override string ToString()
            {
                return string.Format(
                    "  skey={3} service(name={0}, id={1}, typeId={2}) featureId={4} value='{5}'",
                    ServiceName,
                    ServiceId,
                    ServiceTypeId,
                    SubscriptionSkey,
                    FeatureId,
                    Value);
            }
        }

        private static void GetSubscriptionsValue(Database db, int userId, Dictionary<int, FeatureInfo> featureInfo, bool logProcessing)
        {
            var records = SubscriptionRecord.Get(db, userId, featureInfo);

            if (logProcessing && m_log.IsDebugEnabled)
            {
                var recordsText = string.Join(
                    Environment.NewLine,
                    records
                        .OrderBy(x => x.FeatureId)
                        .ThenBy(x => x.SubscriptionSkey)
                        .Select(x => x.ToString()));
                m_log.DebugFormat("subscription records found:{0}{1}", Environment.NewLine, recordsText);
            }

            var recordsByFeatureId = records.ToLookup(x => x.FeatureId);

            foreach (var feature in featureInfo.Values)
            {
                var featureRecords = recordsByFeatureId[feature.Id].ToList();
                if (!featureRecords.Any())
                {
                    if (logProcessing)
                        m_log.DebugFormat(
                            "no active service subscription records found for featureCode={0}. feature value is not defined.",
                            feature.Code);
                    continue;
                }

                try
                {
                    feature.Value = CalculateFeatureValueFromSubscriptions(featureRecords, feature, logProcessing);
                }
                catch (FormatException e)
                {
                    throw new FeatureValueFormatException(userId, feature.Code, e);
                }
            }
        }

        private static string CalculateFeatureValueFromSubscriptions(List<SubscriptionRecord> records, FeatureInfo featureInfo, bool logProcessing)
        {
            string featureValue = null;

            switch (featureInfo.AggregationMethod)
            {
                case FeatureValueAggregationMethod.CAS:
                    records = records
                        .OrderBy(x => x.IsAddon ? 0 : 1)
                        .ThenByDescending(x => x.SubscriptionSkey)
                        .ToList();
                    var record = records.FirstOrDefault(x => x.IsAddon)
                                 ?? records.FirstOrDefault(x => !x.IsAddon);
                    if (record != null)
                    {
                        if (logProcessing)
                            m_log.DebugFormat("Record selected with CAS aggregation method: {0}", record);

                        featureValue = featureInfo.UseQuantity
                            ? NumberToString((record.Quantity ?? 1) * StringToNumber(record.Value))
                            : record.Value;
                    }
                    break;
                case FeatureValueAggregationMethod.Sum:
                    var sum = featureInfo.UseQuantity
                        ? records
                            .Select(x => (x.Quantity ?? 1) * StringToNumber(x.Value))
                            .Aggregate(0.0d, (x, y) => x + y)
                        : records
                            .Select(x => StringToNumber(x.Value))
                            .Aggregate(0.0d, (x, y) => x + y);
                    featureValue = NumberToString(sum);
                    break;
                case FeatureValueAggregationMethod.Min:
                    var min = featureInfo.UseQuantity
                        ? records
                            .Select(x => (x.Quantity ?? 1) * StringToNumber(x.Value))
                            .Min()
                        : records
                            .Select(x => StringToNumber(x.Value))
                            .Min();
                    featureValue = NumberToString(min);
                    break;
                case FeatureValueAggregationMethod.Max:
                    var max = featureInfo.UseQuantity
                        ? records
                            .Select(x => (x.Quantity ?? 1) * StringToNumber(x.Value))
                            .Max()
                        : records
                            .Select(x => StringToNumber(x.Value))
                            .Max();
                    featureValue = NumberToString(max);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        // ReSharper disable once NotResolvedInText
                        "AggregationMethod",
                        featureInfo.AggregationMethod.ToString("G"),
                        "Not defined");
            }

            if (logProcessing)
                m_log.DebugFormat("Feature {0} calculated value: '{1}'", featureInfo.Code, featureValue);

            return featureValue;
        }

        private static double StringToNumber(string value)
        {
            return double.Parse(value);
        }

        private static string NumberToString(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private class FeatureInfo
        {
            public int Id { get; private set; }
            public string Code { get; private set; }
            public FeatureValueAggregationMethod AggregationMethod { get; private set; }
            public bool UseQuantity { get; private set; }
            public string Value { get; set; }
            public int UserOverrideRequiredServiceId { get; set; }

            private FeatureInfo(IDataRecord reader)
            {
                Id = reader.GetInt32(0);
                Code = reader.GetString(1);
                AggregationMethod =
                    reader.IsDBNull(2) ? FeatureValueAggregationMethod.Default : (FeatureValueAggregationMethod)reader.GetInt32(2);
                UseQuantity = !reader.IsDBNull(3) && reader.GetInt32(3) == 1;
                Value = null;
                UserOverrideRequiredServiceId = 0;
            }

            private const string SqlGetFeatureInfoByCode =
                @"  select
                        FEATURE_ID,
                        FEATURE_CODE,
                        AGGREGATION_METHOD_ID,
                        USE_SUBSCRIPTION_QUANTITY
                    from
                        FEATURES
                    where
                        upper(FEATURE_CODE) in ({0})";

            public static Dictionary<int, FeatureInfo> Get(Database db, IReadOnlyCollection<string> codes, bool logProcessing)
            {
                var sql = string.Format(
                    SqlGetFeatureInfoByCode,
                    string.Join(",", Enumerable.Range(0, codes.Count).Select(i => ":p" + i)));
                using (var cmd = new OracleCommand(sql))
                {
                    var i = 0;
                    foreach (var code in codes)
                        cmd.Parameters.Add("p" + i++, OracleDbType.Varchar2).Value = code.ToUpperInvariant();

                    var result = new Dictionary<int, FeatureInfo>();
                    using (var reader = db.ExecuteReader(cmd))
                    {
                        while (reader.Read())
                        {
                            var info = new FeatureInfo(reader);
                            result.Add(info.Id, info);
                        }
                    }

                    var found = new HashSet<string>(result.Values.Select(x => x.Code));
                    var notFound = new HashSet<string>(codes, StringComparer.CurrentCultureIgnoreCase);
                    notFound.ExceptWith(found);
                    if (notFound.Any())
                        throw new FeatureInfoNotFoundException(
                            string.Format(
                                "Feature info not found for codes [{0}]",
                                string.Join(", ", notFound.Select(x => "'" + x + "'"))));

                    if (logProcessing && m_log.IsDebugEnabled)
                    {
                        var features = string.Join(
                            Environment.NewLine,
                            result.Values
                                .Select(
                                    x =>
                                        string.Format(
                                            "{{ code: {0}, id: {1}, am: {2}, useQuantity: {3} }}",
                                            x.Code,
                                            x.Id,
                                            x.AggregationMethod.ToString("G"),
                                            x.UseQuantity)));
                        m_log.DebugFormat("feature info found:{0}{1}", Environment.NewLine, features);
                    }

                    return result;
                }
            }
        }

        private static void GetCustomerOverrideValues(Database db, int userId, Dictionary<int, FeatureInfo> featureInfo, bool logProcessing)
        {
            const string sqlGetCustomerFeatureValue =
                @"  select 
                        cf.feature_id,
                        cf.feature_value,
                        cf.required_service_id
                    from 
                        customer_features cf 
                    where
                        cf.userid = :userId
                        and cf.feature_id in ({0})
                        and nvl(cf.expires_on, sysdate + 1) > sysdate";

            var sql = string.Format(
                sqlGetCustomerFeatureValue,
                string.Join(",", Enumerable.Range(0, featureInfo.Count).Select(i => ":p" + i)));
            using (var cmd = new OracleCommand(sql))
            {
                cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;

                var i = 0;
                foreach (var featureId in featureInfo.Keys)
                    cmd.Parameters.Add("p" + i++, OracleDbType.Int32).Value = featureId;

                using (var reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        var featureId = reader.GetInt32(0);
                        var info = featureInfo[featureId];
                        info.Value = reader.GetString(1);
                        info.UserOverrideRequiredServiceId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    }
                }
            }

            if (logProcessing && m_log.IsDebugEnabled)
            {
                var userOverrideNotFound = featureInfo.Values.Where(x => x.Value == null).ToList();
                if (userOverrideNotFound.Any())
                {
                    var list = string.Join(
                        Environment.NewLine,
                        userOverrideNotFound
                            .Select(
                                x =>
                                    string.Format(
                                        "{{ code: {0}, id: {1} }}",
                                        x.Code,
                                        x.Id)));
                    m_log.DebugFormat("customer override value not found:{0}{1}", Environment.NewLine, list);
                }

                var userOverrideFound = featureInfo.Values.Where(x => x.Value != null).ToList();
                if (userOverrideFound.Any())
                {
                    var list = string.Join(
                        Environment.NewLine,
                        userOverrideFound
                            .Select(
                                x =>
                                    string.Format(
                                        "{{ code: {0}, id: {1}, value: {2}, required_service_id: {3} }}",
                                        x.Code,
                                        x.Id,
                                        x.Value,
                                        x.UserOverrideRequiredServiceId)));
                    m_log.DebugFormat("customer override value found:{0}{1}", Environment.NewLine, list);
                }
            }

            var requiresServiceCheck = featureInfo.Values
                .Where(x => x.UserOverrideRequiredServiceId > 0 && x.Value != null)
                .ToList();
            if (!requiresServiceCheck.Any()) return;

            var requiredServiceIds = requiresServiceCheck
                .Select(x => x.UserOverrideRequiredServiceId)
                .Distinct()
                .ToList();
            var subscribedServiceIds = GetActiveSubscriptions(db, userId, requiredServiceIds);

            if (logProcessing && m_log.IsDebugEnabled && requiresServiceCheck.Any())
            {
                var list = string.Join(
                    Environment.NewLine,
                    requiresServiceCheck
                        .Select(
                            x =>
                                string.Format(
                                    "{{ code: {0}, id: {1}, value: {2}, required_service_id: {3}, userHasActiveSubscription: {4} }}",
                                    x.Code,
                                    x.Id,
                                    x.Value,
                                    x.UserOverrideRequiredServiceId,
                                    subscribedServiceIds.Contains(x.UserOverrideRequiredServiceId) ? FeatureValues.True : FeatureValues.False)));
                m_log.DebugFormat("active subscription check required:{0}{1}", Environment.NewLine, list);
            }

            foreach (var info in requiresServiceCheck.Where(info => !subscribedServiceIds.Contains(info.UserOverrideRequiredServiceId)))
                info.Value = null;
        }

        private static HashSet<int> GetActiveSubscriptions(Database db, int userId, IReadOnlyList<int> requiredServiceIds)
        {
            if (!requiredServiceIds.Any())
                return new HashSet<int>();

            const string getServiceCountSql =
                @"  select
                        ss.service_id,
                        count(*) 
                    from 
                        service_subscription ss
                    where 
                        ss.userid = :userId 
                        and ss.service_id in ({0}) 
                        and nvl(ss.is_deleted, 0) = 0
                    group by
                        ss.service_id";

            var sql = string.Format(
                getServiceCountSql,
                string.Join(",", Enumerable.Range(0, requiredServiceIds.Count).Select(i => ":p" + i)));
            using (var cmd = new OracleCommand(sql))
            {
                cmd.Parameters.Add("userId", OracleDbType.Int32).Value = userId;
                for (var i = 0; i < requiredServiceIds.Count; i++)
                    cmd.Parameters.Add("p" + i, OracleDbType.Int32).Value = requiredServiceIds[i];

                var result = new HashSet<int>();
                using (var reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        var serviceId = reader.GetInt32(0);
                        var count = reader.GetInt32(1);
                        if (count > 0) result.Add(serviceId);
                    }
                }

                return result;
            }
        }
    }
}