using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.ErrorTracker.Web;
using Com.O2Bionics.Utils.Web.ModelBinders;
using JetBrains.Annotations;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;

[assembly: OwinStartup(typeof(Com.O2Bionics.ChatService.Web.Console.Startup))]

namespace Com.O2Bionics.ChatService.Web.Console
{
    public partial class Startup
    {
        private static ILog _log;

        private static readonly WorkspaceSettings m_settings = new JsonSettingsReader().ReadFromFile<WorkspaceSettings>();

        [UsedImplicitly]
        public void Configuration(IAppBuilder app)
        {
            GlobalContainer.RegisterInstance<IIdentifierReader>(new UserIdentifierReader());
            LogConfigurator.Configure(m_settings.ErrorTracker, "Workspace");

            _log = LogManager.GetLogger(typeof(Startup));
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => JsonSerializerBuilder.Default);

            ModelBinders.Binders.DefaultBinder = new BetterDefaultModelBinder();
            ModelBinders.Binders.Add(typeof(HashSet<decimal>), new HashSetDecimalModelBinder());

            app.Use<ErrorTrackerMiddleware>();
            //register other middle-ware.

            ConfigureContainer(app);
            ConfigureAuth(app);
            ConfigureSignalR(app);
            ConfigureEventReceiver(app);

            _log.DebugFormat("{0}{0}{0}started", Environment.NewLine);
        }
    }
}