using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Com.O2Bionics.Chat.Web.Startup))]

namespace Com.O2Bionics.Chat.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
        }
    }
}