using System.Net;
using System.Web;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils.Web
{
    public static class HttpResponseBaseExtensions
    {
        public static void Write([NotNull] this HttpResponseBase response, HttpStatusCode statusCode, string message = null)
        {
            Write(response, (int)statusCode, message);
        }

        private static void Write([NotNull] this HttpResponseBase response, int statusCode, string message = null)
        {
            response.StatusCode = statusCode;
            if (string.IsNullOrEmpty(message))
                return;

            response.Write(message);
            response.TrySkipIisCustomErrors = true;
        }
    }
}