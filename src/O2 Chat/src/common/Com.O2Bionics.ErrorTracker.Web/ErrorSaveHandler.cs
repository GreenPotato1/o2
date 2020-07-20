using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Com.O2Bionics.ErrorTracker.Web.Properties;
using Com.O2Bionics.Utils;

namespace Com.O2Bionics.ErrorTracker.Web
{
    //TODO: p3. use OWIN instead.
    public sealed class ErrorSaveHandler : HttpTaskAsyncHandler
    {
        private const int MaxMessageLength = 1024 * 1024;

        private static readonly ILog m_log = LogManager.GetLogger(typeof(ErrorSaveHandler));

        private readonly IIdentifierReader m_identifierReader;
        private readonly IErrorService m_errorService;

        public ErrorSaveHandler()
        {
            m_errorService = GlobalContainer.Resolve<IErrorService>();
            if (null == m_errorService)
                throw new Exception(string.Format(Resources.CannotResolveType1, typeof(IErrorService)));

            m_identifierReader = GlobalContainer.Resolve<IIdentifierReader>();
            if (null == m_identifierReader)
                throw new Exception(string.Format(Resources.CannotResolveType1, typeof(IIdentifierReader)));
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            try
            {
                var message = await ReadInputStream(context);
                if (message == null) return;

                var errorInfo = ParseErrorInfo(context, message);
                if (errorInfo == null) return;

                errorInfo.LoggerName = nameof(ErrorSaveHandler);
                errorInfo.HostName = Dns.GetHostName();
                errorInfo.FillIdentifiers(m_identifierReader);
                errorInfo.FillFromContext(context);

                m_errorService.Save(errorInfo);
                WriteResponse(context, (int)HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                m_log.Error("Unexpected exception", e);

                WriteResponse(context, (int)HttpStatusCode.InternalServerError, Resources.InternalError);
            }
        }

        private static void WriteResponse(HttpContext context, int code, string message = null)
        {
            context.Response.StatusCode = code;
            if (!string.IsNullOrEmpty(message))
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write(message);
            }
        }

        private static async Task<string> ReadInputStream(HttpContext context)
        {
            string result = null;

            var stream = context.Request.InputStream;
            if (stream.CanRead && 0 < stream.Length)
            {
                if (MaxMessageLength < stream.Length)
                {
                    WriteResponse(
                        context,
                        (int)HttpStatusCode.BadRequest,
                        string.Format(Resources.TooLargeInputError2, stream.Length, MaxMessageLength));
                    return null;
                }

                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    result = await reader.ReadToEndAsync();
                }
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                WriteResponse(context, (int)HttpStatusCode.BadRequest, Resources.SupplyInputData);
                return null;
            }

            return result;
        }

        private static ErrorInfo ParseErrorInfo(HttpContext context, string message)
        {
            try
            {
                var errorInfo = message.JsonUnstringify2<ErrorInfo>();
                if (errorInfo != null)
                    return errorInfo;

                WriteResponse(context, (int)HttpStatusCode.BadRequest, Resources.NullAfterDeserializationError);
            }
            catch (Exception e)
            {
                if (m_log.IsDebugEnabled)
                    m_log.DebugFormat("ParseErrorInfo failed. message: '{0}', exception: '{1}", message, e);
                else
                    m_log.Warn("ParseErrorInfo failed. " + e);

                WriteResponse(context, (int)HttpStatusCode.BadRequest, string.Format(Resources.DeserializingError1, e.Message));
            }

            return null;
        }
    }
}