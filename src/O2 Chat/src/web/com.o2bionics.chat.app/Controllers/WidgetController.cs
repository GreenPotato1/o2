using System.Net;
using System.Web.Mvc;
using Com.O2Bionics.ChatService.Contract.Widget;
using Com.O2Bionics.Utils;
using Com.O2Bionics.ChatService.Web.Console.Properties;

namespace Com.O2Bionics.ChatService.Web.Console.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    public sealed class WidgetController : ManagementControllerBase
    {
        [HttpPost]
        public ActionResult Loads(WidgetLoadRequest request)
        {
            if (null == request)
            {
                Write((int)HttpStatusCode.BadRequest, "Empty request");
                return null;
            }

            request.SetDates(true);
            request.BeginDate = request.BeginDate.RemoveTime();
            request.EndDate = request.EndDate.RemoveTime();
            if (request.EndDate <= request.BeginDate)
            {
                var error = string.Format(
                    Resources.ArgumentNameValueMustBeGreaterThanError4,
                    nameof(request.EndDate),
                    request.EndDate,
                    nameof(request.BeginDate),
                    request.BeginDate);
                Write((int)HttpStatusCode.BadRequest, error);
                return null;
            }

            var diff = request.EndDate.Year - request.BeginDate.Year;
            const int years = 10;
            if (years < diff)
            {
                var error = string.Format(Resources.CannotRequestDataOverYearsError2, diff, years);
                Write((int)HttpStatusCode.BadRequest, error);
                return null;
            }

            var customerId = CustomerId;
            var list = ManagementService.Call(s => s.GetWidgetLoads(customerId, request.BeginDate, request.EndDate));
            return JilJson(list);
        }
    }
}