using System.Web;
using System.Web.Mvc;
using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Web.Console.Controllers
{
    public class UserController : ManagementControllerBase
    {
        [HttpGet]
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Supervisor + "," + RoleNames.Agent)]
        public ActionResult GetAll()
        {
            var result = ManagementService.Call(s => s.GetUsers(CurrentUserId, CustomerId));
            return JilJson(result);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public ActionResult Create(UserInfo user, string password)
        {
            user.CustomerId = CustomerId;
            var result = ManagementService.Call(s => s.CreateUser(CurrentUserId, user, password));
            return JilJson(result);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public ActionResult Update(UserInfo user)
        {
            var currentUserId = CurrentUserId;

            user.CustomerId = CustomerId;
            var updateUserResult = ManagementService.Call(s => s.UpdateUser(currentUserId, user));
            var result = updateUserResult;

            if (result.Status.StatusCode == CallResultStatusCode.Success && result.User.Id == currentUserId)
                UserIdentity.SignIn(HttpContext.GetOwinContext(), result.User, User.Identity);

            return JilJson(result);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public ActionResult SetPassword(uint userId, string password)
        {
            var result = ManagementService.Call(s => s.SetUserPassword(CurrentUserId, CustomerId, userId, password));
            return JilJson(result);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Admin)]
        public ActionResult Delete(uint userId)
        {
            var result = ManagementService.Call(s => s.DeleteUser(CurrentUserId, CustomerId, userId));
            return JilJson(result);
        }
    }
}