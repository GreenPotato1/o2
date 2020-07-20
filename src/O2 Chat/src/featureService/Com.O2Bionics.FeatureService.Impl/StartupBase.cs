using Com.O2Bionics.FeatureService.Impl.DataModel;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.ErrorTracker.Web;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.JsonSettings;
using Com.O2Bionics.Utils.Web;
using JetBrains.Annotations;
using Owin;

namespace Com.O2Bionics.FeatureService.Impl
{
    public class StartupBase
    {
        protected FeatureServiceSettings Settings { get; } = new JsonSettingsReader().ReadFromFile<FeatureServiceSettings>();

        [UsedImplicitly]
        public void Configuration(IAppBuilder app)
        {
            ConfigureEnvironment(app);
            ConfigurePipeline(app);
        }

        protected virtual void ConfigureEnvironment(IAppBuilder app)
        {
            ConfigureContainer(app);
        }

        protected virtual void ConfigurePipeline(IAppBuilder app)
        {
            app.Use<ErrorTrackerMiddleware>();
            app.Use<FeatureServiceMiddleware>();
        }

        private void ConfigureContainer(IAppBuilder app)
        {
            GlobalContainer.RegisterInstance(Settings);
            GlobalContainer.RegisterType<IDatabaseFactory, DatabaseFactory>();
            GlobalContainer.RegisterType<IFeaturesManager, FeaturesManager>();
            GlobalContainer.RegisterType<IFeatureServiceCacheManager, FeatureServiceCacheManager>();
            GlobalContainer.RegisterInstance<IIdentifierReader>(new VisitorIdentifierReader());

            app.ScheduleDisposing();
        }
    }
}