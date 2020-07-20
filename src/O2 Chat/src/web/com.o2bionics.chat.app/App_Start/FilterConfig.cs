using System.Web.Mvc;
using Com.O2Bionics.Utils.Web.Filters;

namespace Com.O2Bionics.ChatService.Web.Console
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new RequireHttpsAttribute());
            filters.Add(new AntiForgeryTokenCheckAttribute());
        }
    }
}