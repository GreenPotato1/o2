using System.IO;
using System.Linq;
using System.Web;
using Com.O2Bionics.Utils.Web;
using Newtonsoft.Json;

namespace Com.O2Bionics.ChatService.Web.Chat.Handlers
{
    public class ThemesHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            if (!CheckCors(context))
                return;

            var maximizedThemes = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/themes/maximized"))
                .GetDirectories()
                .Select(x => x.Name);

            var minimizedThemes = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/themes/minimized"))
                .GetDirectories()
                .Select(x => x.Name);

            HttpContext.Current.Response.Write(
                JsonConvert.SerializeObject(
                    new ThemesDto
                        {
                            Maximized = maximizedThemes,
                            Minimized = minimizedThemes
                        }));
        }

        private bool CheckCors(HttpContext context)
        {
            // use '*' because otherwise we need to hold all domains of all customers
            // TODO: WorkspaceUrl should be enough?
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Cache-Control,X-Requested-With,Content-Type");

            return context.Request.HttpMethod == "GET";
        }
    }
}