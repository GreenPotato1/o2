using System.Web.Mvc;
using System.Web.Routing;

namespace Com.O2Bionics.Chat.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                "404-PageNotFound",
                "{*url}",
                new { controller = "Home", action = "PageNotFound" }
            );
        }
    }
}