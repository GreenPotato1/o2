using System.Web.Optimization;

namespace Com.O2Bionics.ChatService.Web.Console
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Account/_AccountLayout.cshtml
            bundles.Add(
                new StyleBundle("~/bundles/account/css")
                    .Include(

                    ));
            bundles.Add(
                new ScriptBundle("~/bundles/account/js")
                    .Include(

                    ));

            BundleTable.EnableOptimizations = true;
        }
    }
}