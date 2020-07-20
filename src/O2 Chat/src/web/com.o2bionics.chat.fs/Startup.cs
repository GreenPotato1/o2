using Com.O2Bionics.FeatureService.Impl;
using Com.O2Bionics.FeatureService.Web;
using Com.O2Bionics.ErrorTracker;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace Com.O2Bionics.FeatureService.Web
{
    public class Startup : StartupBase
    {
        protected override void ConfigureEnvironment(IAppBuilder app)
        {
            LogConfigurator.Configure(Settings.ErrorTracker, "FeatureService");

            base.ConfigureEnvironment(app);
        }
    }
}