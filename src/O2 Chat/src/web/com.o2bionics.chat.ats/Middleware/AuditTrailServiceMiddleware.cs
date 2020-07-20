using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Com.O2Bionics.AuditTrail.Contract;
using Com.O2Bionics.AuditTrail.Contract.Names;
using Com.O2Bionics.AuditTrail.Contract.Settings;
using Com.O2Bionics.AuditTrail.Web.Properties;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Web;
using JetBrains.Annotations;
using Jil;
using log4net;
using Microsoft.Owin;

namespace Com.O2Bionics.AuditTrail.Web.Middleware
{
    public sealed class AuditTrailServiceMiddleware : OwinMiddleware
    {
        private const string AllowedMethod = "POST";
        private static readonly ILog m_log = LogManager.GetLogger(typeof(AuditTrailServiceMiddleware));

        private readonly IAuditTrailService m_auditTrailService;
        private readonly int m_requestMaxLength;

        public AuditTrailServiceMiddleware(OwinMiddleware next)
            : base(next)
        {
            var settings = GlobalContainer.Resolve<AuditTrailServiceSettings>();
            if (null == settings)
                throw new Exception(string.Format(Resources.TypeMustHaveBeenRegistered1, nameof(AuditTrailServiceSettings)));
            m_requestMaxLength = AuditTrailServiceSettings.RequestMaxLength;

            m_auditTrailService = GlobalContainer.Resolve<IAuditTrailService>();
            if (null == m_auditTrailService)
                throw new Exception(string.Format(Resources.TypeMustHaveBeenRegistered1, nameof(IAuditTrailService)));
        }

        public override async Task Invoke(IOwinContext context)
        {
            var actionName = GetAction(context);

            Func<IOwinContext, string, Task> func;
            switch (actionName)
            {
                case ActionNames.GetFacets:
                    func = GetFacets;
                    break;
                case ActionNames.SaveAudit:
                    func = SaveImpl;
                    break;
                default:
                    await Next.Invoke(context);
                    return;
            }

            try
            {
                if (null == context.Request.Body || 0 == context.Request.Body.Length)
                {
                    Finish(context, (int)HttpStatusCode.BadRequest, Resources.EmptyFormDataError);
                    return;
                }

                if (m_requestMaxLength < context.Request.Body.Length)
                {
                    Finish(context, (int)HttpStatusCode.BadRequest, string.Format(Resources.TooLargeRequest1, context.Request.Body.Length));
                    return;
                }

                var body = await context.GetFormData();
                if (string.IsNullOrEmpty(body))
                {
                    Finish(context, (int)HttpStatusCode.BadRequest, Resources.EmptyFormDataError);
                    return;
                }

                if (m_log.IsDebugEnabled)
                    m_log.Debug($"{actionName}: '{body}'.");

                await func(context, body);
            }
            catch (Exception e)
            {
                m_log.Error("Error in " + actionName, e);
                Finish(context, (int)HttpStatusCode.InternalServerError, Resources.InternalError);
            }
        }

        private static void Finish(IOwinContext context, int code, [CanBeNull] string message = null)
        {
            context.Response.StatusCode = code;
            if (!string.IsNullOrEmpty(message))
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write(message);
            }
        }

        private static string GetAction(IOwinContext context)
        {
            if (AllowedMethod != context.Request.Method)
                return null;

            var path = context.Request.Path.Value;
            var result = VirtualPathUtility.GetFileName(path);
            return result;
        }

        [CanBeNull]
        private T GetRequest<T>([NotNull] IOwinContext context, [NotNull] string body)
            where T : class
        {
            try
            {
                var result = body.JsonUnstringify2<T>();
                if (null == result)
                {
                    Finish(context, (int)HttpStatusCode.BadRequest, Resources.EmptyFormDataError);
                    return null;
                }

                return result;
            }
            catch (Exception e)
            {
                m_log.Warn("Error parsing the request", e);
                Finish(context, (int)HttpStatusCode.BadRequest, Resources.ErrorParsingRequest);
                return null;
            }
        }

        private async Task GetFacets(IOwinContext context, [NotNull] string body)
        {
            var filter = GetRequest<Filter>(context, body);
            if (null == filter)
                return;

            filter.SetDates();
            var error = filter.Validate(false);
            if (!string.IsNullOrEmpty(error))
            {
                Finish(context, (int)HttpStatusCode.BadRequest, error);
                return;
            }

            var response = await m_auditTrailService.SelectFacets(filter);
            var serialized = null == response ? null : JSON.Serialize(response, JsonSerializerBuilder.SkipNullJilOptions);
            var text = string.IsNullOrEmpty(serialized) ? "{}" : serialized;
            await context.Response.WriteAsync(text);
        }

        private async Task SaveImpl([NotNull] IOwinContext context, [NotNull] string body)
        {
            GetHeaders(context.Request.Headers, out var productCode);
            if (string.IsNullOrEmpty(productCode))
            {
                Finish(
                    context,
                    (int)HttpStatusCode.BadRequest,
                    string.Format(Resources.HeaderMustHaveBeenProvided1, HeaderNames.ProductCode));
                return;
            }

            var serializedJson = GetRequest<string>(context, body);
            if (string.IsNullOrEmpty(serializedJson))
            {
                Finish(context, (int)HttpStatusCode.BadRequest, Resources.EmptyFormDataError);
                return;
            }

            await m_auditTrailService.Save(productCode, serializedJson);
        }

        private static void GetHeaders(IHeaderDictionary headers, out string productCode)
        {
            productCode = null;
            if (null == headers)
                return;

            productCode = GetHeader(headers, HeaderNames.ProductCode);
        }

        [CanBeNull]
        private static string GetHeader([NotNull] IHeaderDictionary headers, [NotNull] string key)
        {
            if (headers.TryGetValue(key, out var raw) && null != raw && 1 == raw.Length)
            {
                return raw[0];
            }

            return null;
        }
    }
}