using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Com.O2Bionics.FeatureService.Constants;
using Com.O2Bionics.Utils;
using log4net;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Com.O2Bionics.FeatureService.Impl
{
    public class FeatureServiceMiddleware : OwinMiddleware
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(FeatureServiceMiddleware));

        private static readonly string[] m_allowedMethods = { "GET", "POST", };

        private static readonly JsonSerializer m_serializer = JsonSerializerBuilder.Default;

        public FeatureServiceMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (!m_allowedMethods.Contains(context.Request.Method))
            {
                await Next.Invoke(context);
                return;
            }

            FeatureServiceCallResult result = null;

            var command = VirtualPathUtility.GetFileName(context.Request.Path.Value);
            try
            {
                switch (command)
                {
                    case "get":
                        result = GetFeatureValues(GetFeatureValuesParams.Create(context));
                        break;
                    case "resetCache":
                        result = ResetCache();
                        break;
                    case "ping":
                        result = Ping();
                        break;
                }
            }
            catch (Exception e)
            {
                m_log.Error(nameof(FeatureServiceMiddleware), e);
                result = new FeatureServiceCallResult(e);
            }

#if ERRORTRACKERTEST
            if ("test".Equals(command, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"{nameof(FeatureServiceMiddleware)} passed at {DateTime.UtcNow} - check the Elastic server.");
#endif

            if (result == null)
            {
                await Next.Invoke(context);
                return;
            }

            BuildResponse(context, result);
        }

        private class GetFeatureValuesParams
        {
            public string ProductCode;
            public int UserId;
            public HashSet<string> FeatureCodes;
            public bool? IgnoreCache;
            public int? TimeToLiveSeconds;
            public bool? LogProcessing;
            public bool? LogSqlQuery;

            public static GetFeatureValuesParams Create(IOwinContext context)
            {
                var query = context.Request.Query;

                var productCode = query["pc"];
                if (string.IsNullOrWhiteSpace(productCode))
                    throw new ParameterValidationException("productCode (pc) can't be null or whitespace");
                productCode = productCode.Trim();

                var userIdText = query[ParameterNames.CustomerId];
                if (string.IsNullOrWhiteSpace(userIdText))
                    throw new ParameterValidationException($"userId ({ParameterNames.CustomerId}) can't be null or whitespace");
                int userId;
                if (!int.TryParse(userIdText, out userId))
                    throw new ParameterValidationException(
                        "userId (" + ParameterNames.CustomerId + ") value '{0}' can't be parsed as Int32",
                        userIdText);

                var featureCodesText = query["fc"];
                if (string.IsNullOrWhiteSpace(featureCodesText))
                    throw new ParameterValidationException("featureCodes (fc) can't be null or whitespace");
                var featureCodes = new HashSet<string>(
                    featureCodesText.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()),
                    StringComparer.InvariantCultureIgnoreCase);
                if (!featureCodes.Any())
                    throw new ParameterValidationException("featureCodes (fc) list can't be empty");

                var ignoreCacheText = query["ic"];
                var ignoreCache = string.IsNullOrWhiteSpace(ignoreCacheText) ? (bool?)null : ignoreCacheText == FeatureValues.True;

                var timeToLiveSecondsText = query["ttl"];
                int? timeToLiveSeconds = null;
                if (!string.IsNullOrWhiteSpace(timeToLiveSecondsText))
                {
                    int timeToLiveSecondsValue;
                    if (!int.TryParse(timeToLiveSecondsText, out timeToLiveSecondsValue))
                        throw new ParameterValidationException("timeToLive (ttl) value '{0}' can't be parsed as Int32", timeToLiveSecondsText);
                    timeToLiveSeconds = timeToLiveSecondsValue;
                }

                var logProcessingText = query["logp"];
                bool? logProcessing = null;
                if (!string.IsNullOrWhiteSpace(logProcessingText))
                    logProcessing = logProcessingText == FeatureValues.True;

                var logSqlQueryText = query["logq"];
                bool? logSqlQuery = null;
                if (!string.IsNullOrWhiteSpace(logSqlQueryText))
                    logSqlQuery = logSqlQueryText == FeatureValues.True;

                return new GetFeatureValuesParams
                    {
                        ProductCode = productCode,
                        UserId = userId,
                        FeatureCodes = featureCodes,
                        IgnoreCache = ignoreCache,
                        TimeToLiveSeconds = timeToLiveSeconds,
                        LogProcessing = logProcessing,
                        LogSqlQuery = logSqlQuery,
                    };
            }

            public override string ToString()
            {
                return string.Format(
                    "pc='{0}', " + ParameterNames.CustomerId + "={1}, fc=[{2}], ic={3}, ttl={4}, logp={5}, logq={6}",
                    ProductCode,
                    UserId,
                    string.Join(", ", FeatureCodes.Select(x => "'" + x + "'")),
                    IgnoreCache,
                    TimeToLiveSeconds,
                    LogProcessing,
                    LogSqlQuery);
            }
        }


        /// <summary>
        /// Returns values for (userId, featureCodes).
        /// 
        /// pc - required - productCode, string
        /// uid - required - userId, int
        /// fc - required - featureCode, string[]
        /// ic - optional - ignoreCache, intbool
        /// ttl - optional - time to live, int
        /// logp - optional - log processing, intbool
        /// logq - optional - log sql query, intbool
        /// </summary>
        /// <returns>
        ///  sucess -
        ///   { 
        ///     'featureCode1': 'feature1Value',
        ///     'featureCode2': 'feature2Value',
        ///     ...
        ///   }
        ///  failure -
        ///   {
        ///     'error': 'errorType',
        ///     'errorMessage': 'errorMessage',
        ///   }
        /// </returns>
        private static FeatureServiceCallResult GetFeatureValues(GetFeatureValuesParams pp)
        {
            if (m_log.IsDebugEnabled)
                m_log.DebugFormat("GetFeatureValues({0})", pp);

            var fm = GlobalContainer.Resolve<IFeaturesManager>();
            try
            {
                var values = fm.GetFeatureValue(
                    pp.ProductCode,
                    pp.UserId,
                    pp.FeatureCodes,
                    pp.IgnoreCache,
                    pp.TimeToLiveSeconds,
                    pp.LogProcessing,
                    pp.LogSqlQuery);

                return new FeatureServiceCallResult(values);
            }
            catch (ProductCodeNotFoundException e)
            {
                return new FeatureServiceCallResult(e);
            }
            catch (FeatureInfoNotFoundException e)
            {
                return new FeatureServiceCallResult(e);
            }
            catch (FeatureValueFormatException e)
            {
                return new FeatureServiceCallResult(e);
            }
        }

        /// <summary>
        /// Memory cache reset.
        /// 
        /// no parameters.
        /// </summary>
        /// <returns>
        ///  sucess -
        ///   { 
        ///     'resetCache': 'success'
        ///   }
        ///  failure -
        ///   {
        ///     'error': 'errorType',
        ///     'errorMessage': 'errorMessage',
        ///   }
        /// </returns>
        private static FeatureServiceCallResult ResetCache()
        {
            m_log.DebugFormat("ResetCache");

            var fm = GlobalContainer.Resolve<IFeaturesManager>();

            fm.ResetCache();

            return new FeatureServiceCallResult(new Dictionary<string, string> { { "resetCache", "success" } });
        }

        /// <summary>
        /// Check server status.
        /// 
        /// no parameters.
        /// </summary>
        /// <returns>
        ///  sucess -
        ///   { 
        ///     'productCode1': 'db ok' / 'exception message',
        ///     ...
        ///   }
        ///  failure -
        ///   {
        ///     'error': 'errorType',
        ///     'errorMessage': 'errorMessage',
        ///   }
        /// </returns>
        private static FeatureServiceCallResult Ping()
        {
            m_log.DebugFormat("Ping");

            var fm = GlobalContainer.Resolve<IFeaturesManager>();

            return new FeatureServiceCallResult(fm.Ping());
        }

        private static void BuildResponse(IOwinContext context, FeatureServiceCallResult result)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            SetCachePolicyNoCache(context);

            using (var sw = new StringWriter())
            {
                m_serializer.Serialize(sw, result);

                using (var writer = new StreamWriter(context.Response.Body))
                {
                    var value = sw.ToString();
                    writer.Write(value);
                    writer.Flush();
                    if (m_log.IsDebugEnabled)
                        m_log.DebugFormat("BuildResponse {0}", value);
                }
            }
        }

        private static void SetCachePolicyNoCache(IOwinContext context)
        {
            context.Response.Headers.Add("Cache-Control", new[] { "no-cache", "max-age=0", "must-revalidate", "proxy-revalidate" });
            context.Response.Headers.Add("Pragma", new[] { "no-cache" });
            context.Response.Headers.Add("Expires", new[] { "-1" });
        }
    }
}