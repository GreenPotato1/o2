using System.Web.Mvc;
using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Web.Console.Controllers
{
    public class CannedMessagesController : ManagementControllerBase
    {
        [HttpGet]
        public ActionResult GetUserMessages()
        {
            var result = ManagementService.Call(s => s.GetUserCannedMessages(CustomerId, CurrentUserId));
            return JilJson(result);
        }

        [HttpGet]
        public ActionResult GetDepartmentMessages(uint deptId)
        {
            var result = ManagementService.Call(s => s.GetDepartmentCannedMessages(CustomerId, CurrentUserId, deptId));
            return JilJson(result);
        }

        [HttpPost]
        public ActionResult Create(CannedMessageInfo data)
        {
            if (data.DepartmentId == null)
                data.UserId = CurrentUserId;

            var result = ManagementService.Call(s => s.CreateNewCannedMessage(CustomerId, CurrentUserId, data));
            return JilJson(result);
        }

        [HttpPost]
        public ActionResult Update(CannedMessageInfo data)
        {
            var result = ManagementService.Call(s => s.UpdateCannedMessage(CustomerId, CurrentUserId, data.Id, data));
            return JilJson(result);
        }

        [HttpPost]
        public ActionResult Delete(uint id)
        {
            var result = ManagementService.Call(s => s.DeleteCannedMessage(CustomerId, CurrentUserId, id));
            return JilJson(result);
        }
    }
}