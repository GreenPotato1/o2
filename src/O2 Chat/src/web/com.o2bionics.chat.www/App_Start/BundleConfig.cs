using System.Web.Optimization;

namespace Com.O2Bionics.Chat.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(
                new ScriptBundle("~/bundles/js/jqueryval")
                    .Include(
                        "~/Scripts/jquery.validate*"
                    ));

            bundles.Add(
                new ScriptBundle("~/bundles/js/common")
                    .Include(
                        "~/Scripts/vendor/modernizr-2.8.3.min.js",
                        "~/Scripts/vendor/jquery-1.12.3.min.js",                        
                        "~/Scripts/bootstrap.min.js",
                        "~/Scripts/wow.min.js",
                        "~/Scripts/jquery.meanmenu.js",
                        "~/Scripts/jquery.scrollUp.min.js",
                        "~/Scripts/slick.min.js",
                        "~/Scripts/jquery.nav.js",
                        "~/Scripts/jquery.resize.ex.js",
                        "~/Scripts/waitforimages.js",
                        "~/Scripts/jquery.carousel-3d.min.js",
                        "~/Scripts/plugins.js",                        
                        "~/Scripts/main.js"
                    ));

            bundles.Add(
                new StyleBundle("~/bundles/css/common")
                    .Include(
                        "~/Content/bootstrap.min.css",                        
                        "~/Content/font-awesome.css",
                        "~/Content/jquery.carousel-3d.default.css",
                        "~/Content/slick.css",     
                        "~/Content/jquery-ui.css",
                        "~/Content/meanmenu.min.css",
                        "~/Content/animate.css",
                        "~/Content/material-design-iconic-font.css",
                        "~/Content/material-design-iconic-font.min.css",
                        "~/Content/style.css",
                        "~/Content/responsive.css" 
                    ));

            BundleTable.EnableOptimizations = true;
        }
    }
}