using System.Net;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Widget;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Web;
using log4net;
using Owin;

namespace Com.O2Bionics.ChatService.Web.Chat
{
    public partial class Startup
    {
        private static void ConfigureEventReceiver(IAppBuilder app)
        {
            var ownHostName = Dns.GetHostName();

            app.OnAppDisposing(
                () =>
                    {
                        var client = GlobalContainer.Resolve<ITcpServiceClient<IVisitorChatService>>();
                        client.Call(s => s.Unsubscribe(ownHostName + ":" + m_settings.ChatServiceEventReceiverPort));
                    });

            var tcpServiceClient = GlobalContainer.Resolve<ITcpServiceClient<IVisitorChatService>>();
            var log = LogManager.GetLogger(typeof(Startup));

            RepeatHelper.RunUntilSuccess(
                () => { tcpServiceClient.Call(x => x.Subscribe(ownHostName + ":" + m_settings.ChatServiceEventReceiverPort)); },
                TaskConstants.StartAttempts,
                TaskConstants.StartSleepMs,
                log,
                true);

            var customerCache = GlobalContainer.Resolve<ICustomerCache>();
            customerCache.Load().WaitAndUnwrapException();
        }
    }
}