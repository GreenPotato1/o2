using System;
using System.Net;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.ErrorTracker.Web;
using Com.O2Bionics.Utils;
using log4net;

namespace Com.O2Bionics.ChatService.Web.Console
{
    public class MvcApplication : HttpApplication
    {
        /// <summary>
        /// Use the "GetLog()" method; don't use the field directly.
        /// </summary>
        private static ILog _log;

        private IIdentifierReader m_identifierReader;
        private IErrorService m_errorService;
        private IEmergencyWriter m_emergencyWriter;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            if (null == exception
//Ignore WCF exceptions.
                || exception is FaultException
                || exception is HttpException httpException && httpException.GetHttpCode() == (int)HttpStatusCode.NotFound)
                return;

            try
            {
                const string loggerName = "GlobalWorkspace";
                var errorService = GetErrorService();
                var reader = GetIdentifierReader();
                var log = GetLog();

                errorService.LogExceptionSafe(
                    reader,
                    log,
                    exception,
                    loggerName,
                    info => info.FillFromContext(HttpContext.Current));
            }
            catch (Exception ex)
            {
                try
                {
                    var contents = $"Application_Error: {ex}";
                    GetEmergencyWriter().Report(contents);
                }
                catch
                {
//Ignore
                }
            }
        }

        private static ILog GetLog() => _log ?? (_log = LogManager.GetLogger(typeof(MvcApplication)));
        private IIdentifierReader GetIdentifierReader() => m_identifierReader ?? (m_identifierReader = GlobalContainer.Resolve<IIdentifierReader>());
        private IErrorService GetErrorService() => m_errorService ?? (m_errorService = GlobalContainer.Resolve<IErrorService>());
        private IEmergencyWriter GetEmergencyWriter() => m_emergencyWriter ?? (m_emergencyWriter = GlobalContainer.Resolve<IEmergencyWriter>());
    }
}