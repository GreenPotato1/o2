using Com.O2Bionics.AuditTrail.Web;
using Com.O2Bionics.AuditTrail.Web.Middleware;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.ErrorTracker.Web;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using JetBrains.Annotations;
using Microsoft.Owin;
using Owin;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.Utils.Web;
using log4net;

[assembly: OwinStartup(typeof(Startup))]

namespace Com.O2Bionics.AuditTrail.Web
{
    public sealed class Startup
    {
        [UsedImplicitly]
        public void Configuration(IAppBuilder app)
        {
            var settings = new JsonSettingsReader().ReadFromFile<AuditTrailSettings>();
            GlobalContainer.RegisterInstance(settings.AuditTrailServiceSettings);
            GlobalContainer.RegisterInstance(settings.ErrorTracker);

            GlobalContainer.RegisterInstance<IIdentifierReader>(new FakeIdentifierReader());
            LogConfigurator.Configure(settings.ErrorTracker, "AuditTrail");

            app.Use<ErrorTrackerMiddleware>();
            //register other middle-ware.

            ConfigureContainer(app, settings);

            app.Use<AuditTrailServiceMiddleware>();

            var log = LogManager.GetLogger(typeof(Startup));
            log.Info("Service started.");
        }

        private static void ConfigureContainer(IAppBuilder app, AuditTrailSettings settings)
        {
            var esClient = new EsClient(settings.AuditTrailServiceSettings.ElasticConnection);
            var auditTrailService = new AuditTrailService(settings.AuditTrailServiceSettings, esClient);
            GlobalContainer.RegisterInstance<IAuditTrailService>(auditTrailService);

            app.ScheduleDisposing();
        }
    }
}