using System;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using JetBrains.Annotations;
using log4net;

namespace Com.O2Bionics.Utils.Web.Filters
{
    /// <summary>
    /// If request type is not "GET", there must be: 1) a valid token named 
    /// <see cref="LoginConstants.TokenKey"/>, set in the headers or form data 
    /// <seealso cref="ValidateAntiForgeryTokenAttribute"/>, 2) and a validation
    /// cookie.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class AntiForgeryTokenCheckAttribute : FilterAttribute, IAuthorizationFilter
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(AntiForgeryTokenCheckAttribute));

        public void OnAuthorization([NotNull] AuthorizationContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException(nameof(filterContext));

            var httpContext = filterContext.HttpContext;
            var request = httpContext.Request;
            if ("GET" == request.HttpMethod)
                return;

            try
            {
                CheckCsrf(filterContext, request);
            }
            catch (HttpAntiForgeryException e)
            {
                if (m_log.IsDebugEnabled)
                    m_log.Debug($"Access forbidden, method={request.HttpMethod}.", e);

                filterContext.Result = new HttpStatusCodeResult(
                    HttpStatusCode.Forbidden
#if DEBUG
                    ,
                    "The CSRF token must have been provided."
#endif
                );
            }
        }

        private static void CheckCsrf(AuthorizationContext filterContext, HttpRequestBase request)
        {
            var cookie = request.Cookies[AntiForgeryConfig.CookieName];
            var cookieValue = cookie?.Value;

            var token = request.Headers[LoginConstants.TokenKey];

            if (m_log.IsDebugEnabled)
            {
                var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                var actionName = filterContext.ActionDescriptor.ActionName;

                m_log.Debug(
                    $"{nameof(AntiForgery)}.{nameof(AntiForgery.Validate)} {request.HttpMethod} {controllerName}.{actionName}(), cookie='{cookieValue}', token='{token}'.");
            }

            if (string.IsNullOrEmpty(token))
                AntiForgery.Validate();
            else
                AntiForgery.Validate(cookieValue, token);
        }
    }
}