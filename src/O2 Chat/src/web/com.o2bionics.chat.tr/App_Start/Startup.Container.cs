using System.Runtime.Caching;
using Com.O2Bionics.Elastic;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.PageTracker;
using Com.O2Bionics.PageTracker.Contract;
using Com.O2Bionics.PageTracker.DataModel;
using Com.O2Bionics.PageTracker.Storage;
using Com.O2Bionics.PageTracker.Utilities;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Web;
using Owin;

namespace Com.O2Bionics.ChatService.Web.PageTracker
{
    public partial class Startup
    {
        private static void ConfigureContainer(IAppBuilder app)
        {
            GlobalContainer.RegisterInstance(m_settings);
            GlobalContainer.RegisterInstance(m_settings.FeatureServiceClient);
            GlobalContainer.RegisterType<INowProvider, DefaultNowProvider>();
           
            var cache = MemoryCache.Default;
            GlobalContainer.RegisterInstance<ObjectCache>(cache);

            var nowProvider = GlobalContainer.Resolve<INowProvider>();
            var featureServiceClient = new FeatureServiceClient(m_settings.FeatureServiceClient, cache, nowProvider);
            GlobalContainer.RegisterInstance<IFeatureServiceClient>(featureServiceClient);

            var geoLocationResolver = new MaxMindLocalGeoIpAddressResolver(m_settings);
            GlobalContainer.RegisterInstance<IGeoIpAddressResolver>(geoLocationResolver);

            var userAgentParser = new UserAgentParser();
            GlobalContainer.RegisterInstance<IUserAgentParser>(userAgentParser);

            var useElasticStorage = -1;
            if (0 == ++useElasticStorage)
            {
                var esClient = new EsClient(m_settings.ElasticConnection);
                var idStorage = new IdStorage(m_settings, esClient);
                GlobalContainer.RegisterInstance<IIdStorage>(idStorage);
                GlobalContainer.RegisterType<IIdGenerator, IdGenerator>();

                var pageTrackerEs = new PageTrackerEs(
                    geoLocationResolver,
                    userAgentParser,
                    m_settings,
                    esClient,
                    GlobalContainer.Resolve<IIdGenerator>(),
                    featureServiceClient);
                GlobalContainer.RegisterInstance<IPageTracker>(pageTrackerEs);
            }
            else
            {
                var dbFactory = new DatabaseFactory(m_settings);
                var tracker = new PageTrackerMySql(geoLocationResolver, userAgentParser, dbFactory, featureServiceClient);
                GlobalContainer.RegisterInstance<IPageTracker>(tracker);
            }

            app.ScheduleDisposing();
        }
    }
}