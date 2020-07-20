using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Web.Console.Models.Account;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Web.Models;
using Microsoft.Owin.Security;

namespace Com.O2Bionics.ChatService.Web.Console.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private IAuthenticationManager AuthenticationManager =>
            HttpContext.GetOwinContext().Authentication;

        private static TcpServiceClient<IManagementService> UserManagementService =>
            GlobalContainer.Resolve<TcpServiceClient<IManagementService>>();

        [AllowAnonymous]
        [HttpGet]
        public ActionResult RegisterCustomer(string au)
        {
            var model = new RegisterCustomerViewModel();

            if (au == null)
            {
                var cookie = ControllerContext.HttpContext.Request.Cookies["o2bionicsUserInfo"];
                if (cookie != null) au = cookie.Value;
            }

            if (au != null)
            {
                var encrypted = Convert.FromBase64String(au);
                var key = GlobalContainer.Resolve<WorkspaceSettings>().O2BionicsSite3DesKey;
                var jsonUserInfo = O2Bionics3DesEncryptor.Decrypt(encrypted, key);
                var userInfo = jsonUserInfo.JsonUnstringify2<O2BionicsUserInfo>();

                model.Email = userInfo.Email;
                model.FirstName = userInfo.FirstName;
                model.LastName = userInfo.LastName;

                var cookie = new HttpCookie("o2bionicsUserInfo", au)
                    {
                        HttpOnly = true,
                        Secure = true,
                    };
                ControllerContext.HttpContext.Response.Cookies.Add(cookie);
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult UserAlreadyExists(string email)
        {
            var result = UserManagementService.Call(s => s.IsUserEmailExist(email));
            return Json(!result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult RegisterCustomer(RegisterCustomerViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

//            var userInfo = new UserInfo
//                {
//                    
//                };
//
            var pp = new CreateCustomerParameters
                {
                    UserHostAddress = Request.UserHostAddress,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = model.Password,
                    CustomerName = model.CustomerName,
                    Domains = model.WebSiteAddress,
                    LocalDate = model.LocalDate,
                };
            var result = UserManagementService.Call(s => s.CreateCustomer(pp));


            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl, string email)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginInfo = new LoginParameters
                {
                    Email = model.Email,
                    Password = model.Password,
                    ClientType = Constants.ClientType,
                    ClientVersion = Constants.ClientVersion,
                    ClientAddress = HttpContext.Request.UserHostAddress,
                    ClientLocalDate = model.LocalDate,
                };
            var loginResult = UserManagementService.Call(s => s.Login(loginInfo));
            switch (loginResult.Status)
            {
                case AccountLookupStatus.Success:
                    AuthenticationManager.SignOut();
                    UserIdentity.SignIn(HttpContext.GetOwinContext(), loginResult.User, null, model.RememberMe);
                    return RedirectToLocal(returnUrl);
                case AccountLookupStatus.NotActive:
                    return View(
                        "FailureCondition",
                        new FailureConditionViewModel
                            {
                                Title = "Locked Out",
                                Message = "This account has been locked out, please try again later."
                            });
                case AccountLookupStatus.CustomerNotActive:
                    return View(
                        "FailureCondition",
                        new FailureConditionViewModel
                            {
                                Title = "Customer is not active",
                                Message = "The customer account is not active, please try again later."
                            });
                case AccountLookupStatus.NotFound:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
                default:
                    throw new ArgumentOutOfRangeException("Unsupported status: " + loginResult.Status);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult FailureCondition()
        {
            return View(new FailureConditionViewModel { Title = "Error", Message = "It happens." });
        }

        [HttpPost]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            var protocol = Request.Url?.Scheme ?? "https";
            var resetPasswordLink = Url.Action("ResetPassword", "Account", null, protocol);
            if (ModelState.IsValid)
            {
                var result = UserManagementService.Call(s => s.SendResetPasswordEmail(model.Email, resetPasswordLink));

                if (result.StatusCode == CallResultStatusCode.Success)
                {
                    return RedirectToAction("ForgotPasswordConfirmation", "Account", new { model.Email });
                }

                var message = result.Messages.Count > 0
                    ? result.Messages[0].Message
                    : "Error has occurred.";
                return View("FailureCondition", new FailureConditionViewModel { Title = "Error", Message = message, });
            }

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPasswordConfirmation(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ResetPassword(string code)
        {
            return string.IsNullOrWhiteSpace(code)
                ? View("FailureCondition", new FailureConditionViewModel { Title = "Invalid code", Message = "Provided code is invalid" })
                : View(new ResetPasswordViewModel { Code = code });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = UserManagementService.Call(s => s.ResetPassword(model.Code, model.Password));
            switch (result.Status)
            {
                case ResetPasswordCodeStatus.Success:
                    return RedirectToAction("ResetPasswordConfirmation", "Account");

                case ResetPasswordCodeStatus.CodeNotFoundOrExpired:
                    return View(
                        "FailureCondition",
                        new FailureConditionViewModel
                            {
                                Title = "Invalid code",
                                Message = "Code provided can't be used for password restore.",
                            });
                case ResetPasswordCodeStatus.AccountRemovedOrLocked:
                    return View(
                        "FailureCondition",
                        new FailureConditionViewModel
                            {
                                Title = "Account removed or locked",
                                Message = "Account has been removed or locked.",
                            });
                default:
                    throw new ArgumentOutOfRangeException("Unsupported status: " + result);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult TermsOfService(string returnUrl)
        {
            return View();
        }

        [HttpGet]
        public ActionResult Get()
        {
            var customerId = UserIdentity.GetCustomerId(User.Identity);
            var currentUserId = UserIdentity.GetUserId(User.Identity);

            var result = UserManagementService.Call(s => s.GetUser(customerId, currentUserId));

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Update(UserInfo user)
        {
            var customerId = UserIdentity.GetCustomerId(User.Identity);
            var currentUserId = UserIdentity.GetUserId(User.Identity);

            user.CustomerId = customerId;
            var result = UserManagementService.Call(s => s.UpdateUser(currentUserId, user));

            if (result.Status.StatusCode == CallResultStatusCode.Success && result.User.Id == currentUserId)
                UserIdentity.SignIn(HttpContext.GetOwinContext(), result.User, User.Identity);

            return Json(result);
        }

        [HttpGet]
        public ActionResult GetAvatars()
        {
            var defaultAvatarsFolder = Server.MapPath("~/st/i/avatars/");

            var avatars = Directory.Exists(defaultAvatarsFolder)
                ? Directory.EnumerateFiles(defaultAvatarsFolder)
                    .Select(x => AvatarConstants.DefaultAvatarPrefix + Path.GetFileName(x))
                : new string[0];

            return Json(avatars, JsonRequestBehavior.AllowGet);
        }
    }
}