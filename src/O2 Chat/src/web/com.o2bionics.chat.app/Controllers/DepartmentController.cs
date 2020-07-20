using System.Web.Mvc;
using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Web.Console.Controllers
{
    [Authorize(Roles = RoleNames.Admin)]
    public class DepartmentController : ManagementControllerBase
    {
        [HttpGet]
        public ActionResult GetAll()
        {
            var result = ManagementService.Call(s => s.GetDepartments(CurrentUserId, CustomerId));
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(DepartmentInfo dept)
        {
            dept.CustomerId = CustomerId;
            var result = ManagementService.Call(s => s.CreateDepartment(CurrentUserId, dept));
            return JilJson(result);
        }

        [HttpPost]
        public ActionResult Update(DepartmentInfo dept)
        {
            dept.CustomerId = CustomerId;
            var result = ManagementService.Call(s => s.UpdateDepartment(CurrentUserId, dept));
            return JilJson(result);
        }

        [HttpPost]
        public ActionResult Delete(uint deptId)
        {
            var result = ManagementService.Call(s => s.DeleteDepartment(CurrentUserId, CustomerId, deptId));
            return JilJson(result);
        }
    }
}