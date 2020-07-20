using System;
using System.Web.Mvc;
using Jil;


namespace Com.O2Bionics.Utils.Web
{
    public class JilJsonResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.HttpContext.Response;

            const string applicationJson = "application/json";
            response.ContentType = string.IsNullOrEmpty(ContentType) ? applicationJson : ContentType;
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }

            if (Data != null)
            {
                JSON.Serialize(Data, response.Output, JsonSerializerBuilder.DefaultJilOptions);
            }
            else if (applicationJson == response.ContentType)
            {
                // Write some JSON to pacify clients.
                const string emptyJson = "{}";
                response.Output.Write(emptyJson);
            }
        }
    }
}