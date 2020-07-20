using System.Web.Helpers;
using Com.O2Bionics.ChatService.Web.Chat;
using Com.O2Bionics.ChatService.Widget;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.ErrorTracker.Web;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Com.O2Bionics.ChatService.Web.Chat
{
    public partial class Startup
    {
        private static readonly WidgetSettings m_settings = new JsonSettingsReader().ReadFromFile<WidgetSettings>();

        [UsedImplicitly]
        public void Configuration(IAppBuilder app)
        {
            GlobalContainer.RegisterInstance<IIdentifierReader>(new VisitorIdentifierReader());
            LogConfigurator.Configure(m_settings.ErrorTracker, "Widget");

            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;

            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => JsonSerializerBuilder.Default);

	        app.Use<ErrorTrackerMiddleware>();
	        //register other middle-ware.

            ConfigureContainer(app);
            ConfigureSignalR(app);
            ConfigureEventReceiver(app);
        }
    }
}