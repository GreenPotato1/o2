using System.IO;
using Com.O2Bionics.FeatureService.Impl;
using Com.O2Bionics.Utils;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.Infrastructure;
using Owin;

namespace Com.O2Bionics.FeatureService.SelfHostWeb
{
    public class Startup : StartupBase
    {
        protected override void ConfigurePipeline(IAppBuilder app)
        {
            base.ConfigurePipeline(app);

#if DEBUG
            app.UseErrorPage();
#endif
            ConfigureStaticFiles(app);
        }

        private static void ConfigureStaticFiles(IAppBuilder app)
        {
            var sharedOptions = new SharedOptions
                {
                    FileSystem = new PhysicalFileSystem(Path.Combine(AssemblyHelper.GetExecutingAssemblyPath(), "wwwRoot")),
                    RequestPath = new PathString(""),
                };

            var defaultFileOptions = new DefaultFilesOptions(sharedOptions)
                {
                    DefaultFileNames = { "Default.htm" }
                };
            app.UseDefaultFiles(defaultFileOptions);

            var staticFileOptions = new StaticFileOptions(sharedOptions);
            app.UseStaticFiles(staticFileOptions);
        }
    }
}