using Com.O2Bionics.ChatService.Web.PageTracker;
using Com.O2Bionics.ChatService.Web.PageTracker.Code;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.ErrorTracker.Web;
using Com.O2Bionics.PageTracker;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Com.O2Bionics.ChatService.Web.PageTracker
{
    public partial class Startup
    {
        private static readonly PageTrackerSettings m_settings = new JsonSettingsReader().ReadFromFile<PageTrackerSettings>();

        [UsedImplicitly]
        public void Configuration(IAppBuilder app)
        {
            GlobalContainer.RegisterInstance<IIdentifierReader>(new VisitorIdentifierReader());
            LogConfigurator.Configure(m_settings.ErrorTracker, "PageTracker");

	        app.Use<ErrorTrackerMiddleware>();
	        //register other middle-ware.

            ConfigureContainer(app);

            app.UseCors(CorsOptions.AllowAll);
            app.Use<PageTrackerMiddleware>();
        }
    }
}