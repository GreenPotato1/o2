using System;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using Com.O2Bionics.ErrorTracker.Web.Properties;
using Com.O2Bionics.Utils;
using log4net;
using Microsoft.Owin;

namespace Com.O2Bionics.ErrorTracker.Web
{
    //https://stackoverflow.com/questions/30918649/unhandled-exception-global-handler-for-owin-katana

    /// <inheritdoc />
    /// <summary>
    /// Implement an exception handler in OWIN.
    /// </summary>
    public sealed class ErrorTrackerMiddleware : OwinMiddleware
    {
        private readonly ILog m_log;
        private readonly IIdentifierReader m_identifierReader;
        private readonly IErrorService m_errorService;

        public ErrorTrackerMiddleware(OwinMiddleware next) : base(next)
        {
            m_log = LogManager.GetLogger(typeof(ErrorTrackerMiddleware));
            m_errorService = GlobalContainer.Resolve<IErrorService>();
            if (null == m_errorService)
                throw new Exception(string.Format(Resources.CannotResolveType1, typeof(IErrorService)));

            m_identifierReader = GlobalContainer.Resolve<IIdentifierReader>();
            if (null == m_identifierReader)
                throw new Exception(string.Format(Resources.CannotResolveType1, typeof(IIdentifierReader)));
        }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                await Next.Invoke(context);
            }
            catch (FaultException)
            {
                // WCF faults should be tracked inside a WCF service. So just rethrow them.
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                throw;
            }
            catch (Exception e)
            {
                m_errorService.LogExceptionSafe(
                    m_identifierReader,
                    m_log,
                    e,
                    nameof(ErrorTrackerMiddleware),
                    info =>
                        {
                            info.FillFromContext(context);
                            info.Timestamp = DateTime.UtcNow;
                        });
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                throw;
            }
        }
    }
}