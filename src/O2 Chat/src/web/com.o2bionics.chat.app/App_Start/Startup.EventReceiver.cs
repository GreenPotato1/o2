using System;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Web;
using Owin;

namespace Com.O2Bionics.ChatService.Web.Console
{
    public partial class Startup
    {
        private const int SubscribeEventReceiverRepeatDelayMs = 5000;

        private static long _subscribedCount;

        private static void ConfigureEventReceiver(IAppBuilder app)
        {
            var ownHostName = Dns.GetHostName();

            app.OnAppDisposing(
                () =>
                    {
                        if (Interlocked.Read(ref _subscribedCount) > 0)
                        {
                            try
                            {
                                _log.Debug("Unsubscribing Workspace Event Receiver");
                                var client = GlobalContainer.Resolve<TcpServiceClient<IAgentConsoleService>>();
                                client.Call(s => s.Unsubscribe(ownHostName + ":" + m_settings.ChatServiceEventReceiverPort));
                                _log.Info("Workspace Event Receiver unsubscribed");
                            }
                            catch (EndpointNotFoundException e)
                            {
                                _log.WarnFormat("Unsubscribing Workspace Event Receiver failed: {0}", e.Message);
                            }
                            catch (Exception e)
                            {
                                _log.WarnFormat("Unsubscribing Workspace Event Receiver failed: {0}", e);
                            }
                        }
                    });

            _log.Debug("Scheduling Workspace Event Receiver subscription task");
            HostingEnvironment.QueueBackgroundWorkItem(ct => SubscribeEventReceiver(ownHostName, ct));
        }

        private static async Task SubscribeEventReceiver(string hostName, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        _log.DebugFormat(
                            "Subscribing Workspace Event Receiver to {0}:{1} for {2}:{3}",
                            m_settings.ChatServiceClient.Host,
                            m_settings.ChatServiceClient.Port,
                            hostName,
                            m_settings.ChatServiceEventReceiverPort);
                        GlobalContainer.Resolve<TcpServiceClient<IAgentConsoleService>>()
                            .Call(x => x.Subscribe(hostName + ":" + m_settings.ChatServiceEventReceiverPort));
                        Interlocked.Increment(ref _subscribedCount);
                        _log.Info("Workspace Event Receiver subscribed");
                        return;
                    }
                    catch (EndpointNotFoundException e)
                    {
                        _log.DebugFormat("Subscribing Workspace event receiver failed: {0}", e.Message);
                    }
                    catch (Exception e)
                    {
                        _log.DebugFormat("Subscribing Workspace event receiver failed: {0}", e);
                    }

                    await Task.Delay(SubscribeEventReceiverRepeatDelayMs, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
            }

            _log.Info("Workspace Event Receiver subscription task canceled");
        }
    }
}