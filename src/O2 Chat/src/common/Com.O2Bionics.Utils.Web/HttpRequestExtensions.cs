using System;
using System.Web;
using JetBrains.Annotations;

namespace Com.O2Bionics.Utils.Web
{
    public static class HttpRequestExtensions
    {
        [Pure]
        public static bool IsAjaxRequest([NotNull] this HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return request["X-Requested-With"] == "XMLHttpRequest"
                   || request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}