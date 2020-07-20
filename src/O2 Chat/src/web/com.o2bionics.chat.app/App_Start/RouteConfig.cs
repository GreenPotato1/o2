using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Com.O2Bionics.ChatService.Web.Console.Controllers;

namespace Com.O2Bionics.ChatService.Web.Console
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("st/{*path}");
            routes.IgnoreRoute("Errors/{*path}");
            routes.IgnoreRoute("favicon.ico");

            //It is required for the "ErrorSaveHandler" to work.
            //See the "Web.config" file.
            routes.IgnoreRoute("postError");

            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Home",
                url: "{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { action = string.Join("|", GetControllerActions<HomeController>()) }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }

        private static IEnumerable<string> GetControllerActions<T>() where T : Controller
        {
            return typeof(T)
                .GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public)
                .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                .Where(method => !method.IsDefined(typeof(NonActionAttribute)))
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x);
        }
    }
}