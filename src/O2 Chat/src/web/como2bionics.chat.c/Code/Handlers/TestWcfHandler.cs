#if ERRORTRACKERTEST
using System;
using System.Net;
using System.Web;
using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Web.Chat.Handlers
{
    public sealed class TestWcfHandler : IHttpHandler
    {
        public bool IsReusable => true;

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                context.Response.StatusCode = ErrorTrackerTestHelper.RunTest<IVisitorChatService>(out var message);
                context.Response.Write(message);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "text/plain";
                context.Response.Write($"{nameof(TestWcfHandler)} error: {e}.");
            }
        }
    }
}
#endif
