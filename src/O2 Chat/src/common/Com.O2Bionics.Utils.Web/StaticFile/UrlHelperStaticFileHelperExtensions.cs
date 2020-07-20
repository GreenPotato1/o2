using System;
using System.Web;
using System.Web.Mvc;

namespace Com.O2Bionics.Utils.Web.StaticFile
{
    public static class UrlHelperStaticFileHelperExtensions
    {
        public static HtmlString StaticFile(this UrlHelper helper, string path)
        {
            if (helper == null) throw new ArgumentNullException("helper");
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Can't be null or empty", "path");

            var absoluteWebPath = path.StartsWith("~/")
                ? VirtualPathUtility.ToAbsolute(path)
                : path;

            if (!CdnConfiguration.UseCdn) return new HtmlString(absoluteWebPath);

            var context = helper.RequestContext.HttpContext;
            var proto = context.Request.IsSecureConnection ? "https" : "http";
            var hash = StaticFileHasher.GetFileHash(context.Server.MapPath(absoluteWebPath));
            return new HtmlString(string.Format(@"{0}://{1}{2}?v={3}", proto, CdnConfiguration.CdnDomain, absoluteWebPath, hash));
        }
    }
}