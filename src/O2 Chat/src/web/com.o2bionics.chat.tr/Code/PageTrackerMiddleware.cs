using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.Utils;
using log4net;
using JetBrains.Annotations;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Com.O2Bionics.ChatService.Web.PageTracker.Code
{
    [UsedImplicitly]
    public sealed class PageTrackerMiddleware : OwinMiddleware
    {
        private static readonly string[] m_allowedMethods = { "GET", "POST", "OPTIONS" };

        private static readonly ILog m_log = LogManager.GetLogger(typeof(PageTrackerMiddleware));
        private static readonly JsonSerializer m_serializer = JsonSerializerBuilder.Default;

        //TODO: task-367. What is it - delete?
        //private readonly string m_chatSiteOrigin;

        //TODO: task-367. What is it - delete?
        //private readonly string m_chatConsoleSiteOrigin;

        public PageTrackerMiddleware(OwinMiddleware next)
            : base(next)
        {
            //TODO: task-367. What is it - delete?
            //var settings = GlobalContainer.Resolve<PageTrackerSettings>();
            //m_chatSiteOrigin = settings.WidgetUrl.ToString();
            //m_chatConsoleSiteOrigin = settings.WorkspaceUrl.ToString();
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (!m_allowedMethods.Contains(context.Request.Method))
            {
                await Next.Invoke(context);
                return;
            }

            context.Response.ContentType = "text/javascript";
            SetCachePolicy(context);

            object responseObject = null;
            string command = null;
            var status = HttpStatusCode.OK;
            try
            {
                command = VirtualPathUtility.GetFileName(context.Request.Path.Value);
                if (command == PageTrackerConstants.AddCommand)
                {
                    responseObject = await AddHistoryEntry(context);
                }
                else if (command == PageTrackerConstants.GetCommand)
                {
                    responseObject = await GetHistory(context);
                }
            }
            catch (ParameterValidationException e)
            {
                status = HttpStatusCode.BadRequest;
                responseObject = new { error = e.Message };
            }
            catch (Exception e)
            {
                status = HttpStatusCode.InternalServerError;
                m_log.Error($"command='{command.LimitLength()}'.", e);
                responseObject = new { error = "Internal server error." };
            }

#if ERRORTRACKERTEST
            if ("test".Equals(command, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"{nameof(PageTrackerMiddleware)} passed at {DateTime.UtcNow} - check the Elastic server.");
#endif
            context.Response.StatusCode = (int)status;

            using (var sw = new StringWriter())
            {
                m_serializer.Serialize(sw, responseObject);

                using (var writer = new StreamWriter(context.Response.Body))
                {
                    await writer.WriteAsync(sw.ToString());
                    await writer.FlushAsync();
                }
            }

            await Next.Invoke(context);
        }

        //TODO: task-367. What is it - delete?
        //private void SetAccessControlPolicy(IOwinContext context, string origin)
        //{
        //    context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { origin });
        //    context.Response.Headers.Add("Access-Control-Allow-Methods", new[] { string.Join(",", m_allowedMethods) });
        //    context.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Content-Type" });
        //    context.Response.Headers.Add("Access-Control-Request-Headers", new[] { "X-Requested-With,accept,content-type" });
        //}

        private static void SetCachePolicy(IOwinContext context)
        {
            context.Response.Headers.Add("Cache-Control", new[] { "no-cache", "max-age=0", "must-revalidate", "proxy-revalidate" });
            context.Response.Headers.Add("Pragma", new[] { "no-cache" });
            context.Response.Headers.Add("Expires", new[] { "-1" });
        }

        /// <summary>
        /// cid - required - Customer Id  
        /// 
        /// vid - required - Visitor Id serialized as string 
        /// 
        /// veid - optional - Visitor External Id serialized as string
        /// 
        /// tzof - required - client timezone offset to UTC in minutes 
        /// 
        /// tzde - required - client timezone description from user's browser 
        /// 
        /// u - required - page URL
        /// </summary>
        /// <returns>
        /// Json encoded object of two fields: 
        /// 
        ///    string hid - Page History Id
        /// 
        ///    ulong vid - Visitor Id
        /// </returns>
        private static async Task<AddRecordResult> AddHistoryEntry(IOwinContext context)
        {
            var form = await context.Request.ReadFormAsync();
            var query = context.Request.Query;

            var customerIdText = GetParam(query, form, PageTrackerConstants.CustomerIdParameterName);
            if (string.IsNullOrWhiteSpace(customerIdText))
                throw new ParameterValidationException($"CustomerId ({PageTrackerConstants.CustomerIdParameterName}) is required");

            if (!uint.TryParse(customerIdText, out var customerId))
                throw new ParameterValidationException(
                    "CustomerId (" + PageTrackerConstants.CustomerIdParameterName + ") '{0}' can't be parsed as UInt32",
                    customerIdText);
            if (customerId <= 0)
                throw new ParameterValidationException("CustomerId (" + customerId + ") must be positive");

            var visitorIdText = GetParam(query, form, PageTrackerConstants.VisitorIdParameterName);
            if (string.IsNullOrWhiteSpace(visitorIdText))
                throw new ParameterValidationException($"VisitorId ({PageTrackerConstants.VisitorIdParameterName}) is required");
            if (!ulong.TryParse(visitorIdText, out var visitorId))
                throw new ParameterValidationException(
                    "VisitorId (" + PageTrackerConstants.VisitorIdParameterName + ") '{0}' can't be parsed as ulong",
                    visitorIdText);

            var timeZoneOffsetText = GetParam(query, form, "tzof");
            if (string.IsNullOrWhiteSpace(timeZoneOffsetText))
                throw new ParameterValidationException("TimeZone Offset (tzof) is required");
            if (!int.TryParse(timeZoneOffsetText, out var timeZoneOffset))
                throw new ParameterValidationException("TimeZone Offset (tzof) '{0}' can't be parsed as Int32", timeZoneOffsetText);

            var timeZoneDescription = GetParam(query, form, "tzde");
            if (string.IsNullOrWhiteSpace(timeZoneDescription))
                throw new ParameterValidationException("TimeZone Description (tzde) is required");

            var urlText = GetParam(query, form, "u");
            if (string.IsNullOrWhiteSpace(urlText))
                throw new ParameterValidationException("Url (u) is required");
            if (!Uri.TryCreate(urlText, UriKind.Absolute, out var url))
                throw new ParameterValidationException("Url (u) '{0}' can't be parsed as absolute url", urlText);

            var ipText = GetIpAddress(context);
            if (string.IsNullOrWhiteSpace(ipText))
                throw new ParameterValidationException("Client IP address is required");
            if (!IPAddress.TryParse(ipText, out var ipAddress))
                throw new ParameterValidationException("Client IP address '{0}' can't be parsed as IPAddress", ipText);

            var request = new AddRecordArgs
                {
                    CustomerId = customerId,
                    VisitorId = visitorId,
                    VisitorExternalId = GetParam(query, form, "veid"),
                    TimeZone = new TimeZoneDescription(timeZoneOffset, timeZoneDescription),
                    Url = url,
                    CustomText = GetParam(query, form, "ct"),
                    Ip = ipAddress,
                    UserAgentString = context.Request.Headers["User-Agent"],
                };
            var pageTracker = GlobalContainer.Resolve<IPageTracker>();
            var utcNow = DateTime.UtcNow;
            var result = await pageTracker.Add(utcNow, request);
            return result;
        }

        /// <summary>
        /// vid - required - Visitor Id serialized as string
        /// hid - required - historyId
        /// </summary>
        /// <returns>
        /// Json encoded array of <see cref="PageHistoryRecord" />
        /// </returns>
        [ItemNotNull]
        private static async Task<GetHistoryResult> GetHistory(IOwinContext context)
        {
            var customerId = GetCustomerId(context);
            var visitorId = GetVisitorId(context);
            var pageSize = GetPageSize(context);
            var searchPosition = GetSearchPosition(context);

            var pageTracker = GlobalContainer.Resolve<IPageTracker>();
            var result = await pageTracker.Get(customerId, visitorId, pageSize, searchPosition);
            return result;
        }

        private static uint GetCustomerId(IOwinContext context)
        {
            var customerIdText = context.Request.Query[PageTrackerConstants.CustomerIdParameterName];
            if (string.IsNullOrWhiteSpace(customerIdText))
                throw new ParameterValidationException($"CustomerId ({PageTrackerConstants.CustomerIdParameterName}) is required");
            if (!uint.TryParse(customerIdText, out var customerId))
                throw new ParameterValidationException(
                    "CustomerId (" + PageTrackerConstants.CustomerIdParameterName + ") '{0}' can't be parsed as uint",
                    customerIdText);
            if (customerId <= 0)
                throw new ParameterValidationException("CustomerId must be set.");
            return customerId;
        }

        private static ulong GetVisitorId(IOwinContext context)
        {
            var visitorIdText = context.Request.Query[PageTrackerConstants.VisitorIdParameterName];
            if (string.IsNullOrWhiteSpace(visitorIdText))
                throw new ParameterValidationException($"VisitorId ({PageTrackerConstants.VisitorIdParameterName}) is required");
            if (!ulong.TryParse(visitorIdText, out var visitorId))
                throw new ParameterValidationException(
                    "VisitorId (" + PageTrackerConstants.VisitorIdParameterName + ") '{0}' can't be parsed as ulong",
                    visitorIdText);
            if (visitorId <= 0)
                throw new ParameterValidationException("VisitorId must be set.");
            return visitorId;
        }

        private static int GetPageSize(IOwinContext context)
        {
            var pageSize = GetUlong(context, PageTrackerConstants.PageSizeParameterName, "PageSize");
            if (0 == pageSize || PageTrackerConstants.PageHistoryGetLimitMax < pageSize)
                throw new ParameterValidationException(
                    $"PageSize ({PageTrackerConstants.PageSizeParameterName}) '{pageSize}' must not exceed {PageTrackerConstants.PageHistoryGetLimitMax} and be positive.");
            return (int)pageSize;
        }

        private static ulong GetUlong(IOwinContext context, [NotNull] string key, [NotNull] string name)
        {
            ulong id = 0;
            var text = context.Request.Query[key];
            if (!string.IsNullOrEmpty(text) && !ulong.TryParse(text, out id))
                throw new ParameterValidationException(
                    name + " (" + key + ") '{0}' can't be parsed as ulong",
                    text);
            return id;
        }

        [CanBeNull]
        private static SearchPositionInfo GetSearchPosition(IOwinContext context)
        {
            var value = context.Request.Query[PageTrackerConstants.SearchPositionParameterName];
            if (string.IsNullOrEmpty(value))
                return null;

            const int maxLength = 1024;
            if (value.Length > maxLength)
                throw new ParameterValidationException($"{PageTrackerConstants.SearchPositionParameterName} is too long ({maxLength}).");

            var parts = value.Split('|');
            return new SearchPositionInfo(parts);
        }

        private static string GetIpAddress(IOwinContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                var addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.RemoteIpAddress;
        }

        private static string GetParam(IReadableStringCollection query, IFormCollection form, string name)
        {
            return query[name] ?? form[name];
        }
    }
}