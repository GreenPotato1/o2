using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using log4net;

namespace Com.O2Bionics.MailerService.Web
{
    //TODO: p2. task-359. Use a base class HttpApplicationBase.
    public class Global : HttpApplication
    {
        private ILog m_log;
        private ILog Log => m_log ?? (m_log = LogManager.GetLogger(typeof(Global)));

        protected void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            Startup.ConfigureContainer();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            Log.Info("Service started.");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            try
            {
                var exception = Server.GetLastError();
                if (null == exception)
                    return;

                Log.Error(nameof(Application_Error), exception);
            }
            catch
            {
//Ignore
            }
        }
    }
}