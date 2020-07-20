using Microsoft.AspNet.SignalR;
using Owin;

namespace Com.O2Bionics.ChatService.Web.Chat
{
    public partial class Startup
    {
        private const string SignalRPath = "/signalr";

        private static void ConfigureSignalR(IAppBuilder app)
        {
            GlobalHost.Configuration.DefaultMessageBufferSize = 500;
            GlobalHost.Configuration.ConnectionTimeout = m_settings.WebSocket.ConnectionTimeout;
            GlobalHost.Configuration.DisconnectTimeout = m_settings.WebSocket.DisconnectTimeout;
            GlobalHost.Configuration.KeepAlive = m_settings.WebSocket.KeepAlive;

            var hubConfiguration = new HubConfiguration
                {
                    EnableDetailedErrors = true,
                    EnableJavaScriptProxies = false,
                };
            app.MapSignalR(SignalRPath, hubConfiguration);
        }
    }
}