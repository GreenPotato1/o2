using System.Runtime.Caching;
using Com.O2Bionics.AuditTrail.Client;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Contract.AuditTrail;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.ErrorTracker.Web;
using Com.O2Bionics.FeatureService.Client;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using Com.O2Bionics.Utils.Web;
using Owin;

namespace Com.O2Bionics.ChatService.Web.Console
{
    public partial class Startup
    {
        private static void ConfigureContainer(IAppBuilder app)
        {
            GlobalContainer.RegisterInstance(m_settings);
            GlobalContainer.RegisterInstance(m_settings.FeatureServiceClient);
            GlobalContainer.RegisterType<INowProvider, DefaultNowProvider>();

            var eventReceiver = new AgentConsoleEventReceiver();
            var eventReceiverHost = new TcpServiceHost<IAgentConsoleEventReceiver>(eventReceiver, m_settings.ChatServiceEventReceiverPort);
            GlobalContainer.RegisterInstance(eventReceiverHost);

            var identifierReader = GlobalContainer.Resolve<IIdentifierReader>();
            var headerContextFactory = new HeaderContextFactory(identifierReader);
            GlobalContainer.RegisterInstance<IHeaderContextFactory>(headerContextFactory);

            var agentConsoleService = new TcpServiceClient<IAgentConsoleService>(
                m_settings.ChatServiceClient.Host,
                m_settings.ChatServiceClient.Port,
                headerContextFactory);
            GlobalContainer.RegisterInstance(agentConsoleService);

            var userManagementService = new TcpServiceClient<IManagementService>(
                m_settings.ChatServiceClient.Host,
                m_settings.ChatServiceClient.Port,
                headerContextFactory);
            GlobalContainer.RegisterInstance(userManagementService);


            var featureServiceClient = new FeatureServiceClient(m_settings.FeatureServiceClient, MemoryCache.Default, GlobalContainer.Resolve<INowProvider>());
            GlobalContainer.RegisterInstance<IFeatureServiceClient>(featureServiceClient);

            GlobalContainer.RegisterInstance(m_settings.AuditTrailClient);

            var auditTrailClient = new AuditTrailClient(m_settings.AuditTrailClient, GlobalContainer.Resolve<INowProvider>(), ProductCodes.Chat);
            GlobalContainer.RegisterInstance<IAuditTrailClient>(auditTrailClient);

            app.ScheduleDisposing();
        }
    }
}