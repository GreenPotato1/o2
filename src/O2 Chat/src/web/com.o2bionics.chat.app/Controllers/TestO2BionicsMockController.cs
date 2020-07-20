using System;
using System.Web.Mvc;
using Com.O2Bionics.ChatService.Web.Console.Models.TestO2BionicsMock;
using Com.O2Bionics.Utils;
using JetBrains.Annotations;
using Jil;

namespace Com.O2Bionics.ChatService.Web.Console.Controllers
{
    public class O2BionicsUserInfo
    {
        [JilDirective("created")]
        public DateTime CreatedTimeUtc { [UsedImplicitly] get; set; }

        [JilDirective("userId")]
        public int UserId { [UsedImplicitly] get; set; }

        [JilDirective("accountNumber")]
        public string AccountNumber { [UsedImplicitly] get; set; }

        [JilDirective("email")]
        public string Email { get; set; }

        [JilDirective("firstName")]
        public string FirstName { get; set; }

        [JilDirective("lastName")]
        public string LastName { get; set; }
    }

    public class TestO2BionicsMockController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ChatSource()
        {
            var model = new ChatSourceViewModel
                {
                    Email = "test1@some.site.com",
                    FirstName = "Vasily",
                    LastName = "Pupkin",
                    O2BionicsUserId = 12345,
                    O2BionicsAccountNumber = "001234598",
                };

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ChatSource(ChatSourceViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userInfo = new O2BionicsUserInfo
                {
                    CreatedTimeUtc = DateTime.UtcNow,
                    UserId = model.O2BionicsUserId,
                    AccountNumber = model.O2BionicsAccountNumber,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                };
            var jsonUserInfo = userInfo.JsonStringify2();
            var key = GlobalContainer.Resolve<WorkspaceSettings>().O2BionicsSite3DesKey;
            var encrypted = O2Bionics3DesEncryptor.Enrypt(jsonUserInfo, key);
            var encryptedString = Convert.ToBase64String(encrypted);

            return RedirectToAction("RegisterCustomer", "Account", new { au = encryptedString });
        }
    }
}