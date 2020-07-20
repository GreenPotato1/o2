using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Widget;
using Com.O2Bionics.ErrorTracker;
using Com.O2Bionics.ErrorTracker.Web;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Network;
using Com.O2Bionics.Utils.Web;
using Owin;

namespace Com.O2Bionics.ChatService.Web.Chat
{
    public partial class Startup
    {
        private static void ConfigureContainer(IAppBuilder app)
        {
            GlobalContainer.RegisterInstance(m_settings);

            var customerCache = new CustomerCache();
            GlobalContainer.RegisterInstance<ICustomerCache>(customerCache);

            var eventReceiver = new VisitorChatEventReceiver(customerCache);
            var eventReceiverHost = new TcpServiceHost<IVisitorChatEventReceiver>(eventReceiver, m_settings.ChatServiceEventReceiverPort);
            GlobalContainer.RegisterInstance(eventReceiverHost);

            var identifierReader = GlobalContainer.Resolve<IIdentifierReader>();
            var headerContextFactory = new HeaderContextFactory(identifierReader);
            GlobalContainer.RegisterInstance<IHeaderContextFactory>(headerContextFactory);

            var serviceClient = new TcpServiceClient<IVisitorChatService>(
                m_settings.ChatServiceClient.Host,
                m_settings.ChatServiceClient.Port,
                headerContextFactory);
            GlobalContainer.RegisterInstance<ITcpServiceClient<IVisitorChatService>>(serviceClient);
            GlobalContainer.RegisterInstance(serviceClient);

            app.ScheduleDisposing();
        }
    }
}