using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

namespace Com.O2Bionics.Utils.Web.StaticFile
{
    public static class HtmlHelperBundleHelperExtensions
    {
        public static IHtmlString RenderScriptBundle(this HtmlHelper helper, params string[] paths)
        {
            var context = helper.ViewContext.RequestContext.HttpContext;
            var proto = context.Request.IsSecureConnection ? "https" : "http";
            var tagFormat = CdnConfiguration.UseCdn
                ? string.Format(@"<script src='{0}://{1}{{0}}'></script>", proto, CdnConfiguration.CdnDomain)
                : Scripts.DefaultTagFormat;
            return Scripts.RenderFormat(tagFormat, paths);
        }

        public static IHtmlString RenderStyleBundle(this HtmlHelper helper, params string[] paths)
        {
            var context = helper.ViewContext.RequestContext.HttpContext;
            var proto = context.Request.IsSecureConnection ? "https" : "http";
            var tagFormat = CdnConfiguration.UseCdn
                ? string.Format(@"<link href='{0}://{1}{{0}}' rel='stylesheet'/>", proto, CdnConfiguration.CdnDomain)
                : Styles.DefaultTagFormat;
            return Styles.RenderFormat(tagFormat, paths);
        }
    }
}