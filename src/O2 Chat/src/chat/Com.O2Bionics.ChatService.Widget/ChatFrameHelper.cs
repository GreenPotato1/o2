using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Web;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Widget.Properties;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using Com.O2Bionics.Utils.Web;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.ChatService.Widget
{
    public static class ChatFrameHelper
    {
        private const string ErrorPrefix = "<script type=\"text/javascript\">console.error(\"";
        private const string ErrorSuffix = "\");</script>";

        private const string VisitorIdCookieNameBase = "v_";

        public static ulong GetVisitorId(HttpContextBase context, uint customerId)
        {
            // see http://stackoverflow.com/questions/389456/cookie-blocked-not-saved-in-iframe-in-internet-explorer
            context.Response.Headers.Add("P3P", "CP=\"Potato\"");

            var cookieName = VisitorIdCookieNameBase + customerId;
            var cookie = context.Request.Cookies[cookieName];
            if (cookie == null)
                return 0u;

            if (!ulong.TryParse(cookie.Value, out var visitorId))
                return 0u;

            cookie.Expires = DateTime.Now.AddDays(365 * 10);
            cookie.Path = context.Request.Url.GetFolder();
            context.Response.Cookies.Add(cookie);
            return visitorId;
        }

        [CanBeNull]
        public static ChatFrameLoadResult Load(
            [NotNull] HttpRequestBase request,
            uint customerId,
            ulong visitorId,
            DateTime date,
            bool isDemoMode,
            Uri workspaceUri,
            [NotNull] HttpResponseBase response)
        {
            if (0 == customerId)
            {
                //Probably malicious DOS attack.
                WriteError(response, string.Format(Resources.CustomerMustBeActiveError1, 0));
                return null;
            }

            var domain = GetDomain(request, customerId, visitorId, response);
            if (null == domain)
                return null;

            var cache = GlobalContainer.Resolve<ICustomerCache>();
            if (!cache.IsActive(customerId))
            {
                WriteError(response, string.Format(Resources.CustomerMustBeActiveError1, customerId));
                return null;
            }

            date = date.RemoveTime();
            if (isDemoMode)
            {
                if (null == workspaceUri)
                    throw new ArgumentException(nameof(workspaceUri));
                if (!domain.Equals(workspaceUri.Authority, StringComparison.OrdinalIgnoreCase))
                {
                    WriteError(response, string.Format(Resources.RefererDomainMustBeWorkspaceDomainError2, domain, workspaceUri.Authority));
                    return null;
                }
            }
            else if (cache.TryGet(date, customerId, out var entry) && null != entry &&
                     !CheckCacheEntry(customerId, response, entry, domain))
            {
                return null;
            }

            var client = GlobalContainer.Resolve<ITcpServiceClient<IVisitorChatService>>();
            var loadResult = client.Call(s => s.ChatWindowLoad(customerId, visitorId, domain, isDemoMode));
            if (!CheckLoadResult(cache, customerId, date, domain, loadResult, response))
            {
                return null;
            }

            //Enable the page.
            return loadResult;
        }

        [CanBeNull]
        private static string GetDomain([NotNull] HttpRequestBase request, uint customerId, ulong visitorId, [NotNull] HttpResponseBase response)
        {
            string result = null;
            try
            {
                var referrer = request.UrlReferrer;
                if (null != referrer)
                {
                    result = referrer.Authority;
                }
            }
            catch (Exception e)
            {
                try
                {
                    var log = LogManager.GetLogger(typeof(ChatFrameHelper));
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Customer={0} Visitor={1} did not provide a valid domain, Error='{2}'.", customerId, visitorId, e);
                    }
                }
                catch
                {
                    //Ignore
                }
            }

            if (string.IsNullOrEmpty(result))
            {
                //Probably malicious DOS attack.
                WriteError(response, Resources.CannotDetermineDomainError);
                return null;
            }

            if (ServiceConstants.MaximumDomainLength < result.Length)
            {
                //Probably malicious DOS attack.
                var error = string.Format(Resources.DomainIsTooLongError1, result.Length);
                WriteError(response, error);
                return null;
            }

            return result;
        }

        private static bool CheckCacheEntry(
            uint customerId,
            [NotNull] HttpResponseBase response,
            [NotNull] CustomerEntry entry,
            [NotNull] string domain)
        {
            if (!entry.Active)
            {
                var error = string.Format(Resources.CustomerMustBeActiveError1, customerId);
                WriteError(response, error);
                return false;
            }

            if (entry.ViewCounterExceeded)
            {
                WriteError(response, Resources.TooManyChatWidgetLoads);
                return false;
            }

            if (null != entry.UnknownDomains && entry.UnknownDomains.ContainsKey(domain))
            {
                var error = string.Format(Resources.WidgetLoadedFromUnregisteredDomainError1, domain);
                WriteError(response, error);
                return false;
            }

            if (entry.UnknownDomainNumberExceeded && null != entry.Domains && !DomainUtilities.HasDomain(entry.Domains, domain))
            {
                WriteError(response, Resources.TooManyUnknownDomainsError);
                return false;
            }

            return true;
        }

        private static bool CheckLoadResult(
            ICustomerCache cache,
            uint customerId,
            DateTime date,
            [NotNull] string domain,
            ChatFrameLoadResult loadResult,
            [NotNull] HttpResponseBase response)
        {
            string error;
            switch (loadResult.Code)
            {
                case WidgetLoadSatusCode.Allowed:
                    return true;
                case WidgetLoadSatusCode.DisabledCustomer:
                    error = string.Format(Resources.CustomerMustBeActiveError1, customerId);
                    cache.SetSoft(date, customerId, entry => entry.Active = false);
                    break;
                case WidgetLoadSatusCode.ViewCounterExceeded:
                    error = Resources.TooManyChatWidgetLoads;
                    cache.SetSoft(date, customerId, entry => entry.ViewCounterExceeded = true);
                    break;
                case WidgetLoadSatusCode.UnknownDomain:
                    error = string.Format(Resources.WidgetLoadedFromUnregisteredDomainError1, domain);
                    AddUnknownDomain(
                        date,
                        customerId,
                        cache,
                        loadResult.CustomerSettings?.ChatWidgetAppearanceInfo?.Domains,
                        domain);
                    break;
                case WidgetLoadSatusCode.UnknownDomainNumberExceeded:
                    error = Resources.TooManyUnknownDomainsError;
                    cache.SetSoft(date, customerId, entry => entry.UnknownDomainNumberExceeded = true);
                    break;
                default:
#if DEBUG
                    throw new Exception($"Unknown {nameof(WidgetLoadSatusCode)}={loadResult.Code}");
#else
                    error = Resources.ChatWidgetInternalError;
                    break;
#endif
            }

            if (!string.IsNullOrEmpty(error))
                WriteError(response, error);

            return false;
        }

        private static void AddUnknownDomain(
            DateTime date,
            uint customerId,
            ICustomerCache cache,
            [CanBeNull] string domainList,
            [NotNull] string domain)
        {
            if (string.IsNullOrEmpty(domainList))
            {
                Debug.Fail($"customerId={customerId} must have domains.");
                // ReSharper disable once HeuristicUnreachableCode
                return;
            }

            var domains = DomainUtilities.GetDomains(domainList);

            cache.SetSoft(
                date,
                customerId,
                entry =>
                    {
                        entry.Domains = domains;
                        if (null == entry.UnknownDomains)
                        {
                            var domainSet = new ConcurrentHashSet<string>(domain);
                            var compare = Interlocked.CompareExchange(ref entry.UnknownDomains, domainSet, null);
                            if (null == compare)
                            {
                                Debug.Assert(null != entry.UnknownDomains);
                                return;
                            }

                            if (null == entry.UnknownDomains)
                            {
                                Debug.Fail($"Cannot set not null {nameof(entry.UnknownDomains)}.");
                                // ReSharper disable once HeuristicUnreachableCode
                                return;
                            }
                        }

                        if (entry.UnknownDomains.Count < DomainUtilities.DefaultMaximumUnknownDomains)
                            entry.UnknownDomains.TryAdd(domain);
                    });
        }

        public static string FormatError([NotNull] string error)
        {
            Debug.Assert(!string.IsNullOrEmpty(error));
            var result = ErrorPrefix + HttpUtility.JavaScriptStringEncode(error) + ErrorSuffix;
            return result;
        }

        private static void WriteError([NotNull] HttpResponseBase response, [NotNull] string error)
        {
            Debug.Assert(!string.IsNullOrEmpty(error));
            var message = FormatError(error);
            response.Write(HttpStatusCode.BadRequest, message);
        }
    }
}