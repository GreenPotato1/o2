using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace Com.O2Bionics.Utils.Web
{
    public static class OwinContextExtensions
    {
        public static async Task<string> GetFormData(this IOwinContext context)
        {
            var stream = context.Request.Body;
            stream.Position = 0;

            string body;
            using (var reader = new StreamReader(stream))
            {
                body = await reader.ReadToEndAsync();
            }

            stream.Position = 0;
            return body;
        }
    }
}