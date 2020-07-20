using System;
using System.Diagnostics;
using System.Net;
using System.Web;
using Com.O2Bionics.Utils.Web;

namespace Com.O2Bionics.ChatService.Web.Chat.Handlers
{
    public class CaptchaImageHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var hash = context.Request.QueryString["h"];
            Debug.WriteLine("requested hash: '" + hash + "'");

            try
            {
                context.Response.ContentType = "image/png";
                context.Response.BinaryWrite(Captcha.CreatePngImage(hash));
            }
            catch (Exception e)
            {
                Trace.WriteLine("CAPTCHA exception " + e);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "text/plain";
                context.Response.Write("Invalid CAPTCHA hash.");
            }
        }
    }
}