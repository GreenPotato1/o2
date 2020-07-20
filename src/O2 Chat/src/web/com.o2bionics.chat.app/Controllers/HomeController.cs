
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Mvc;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using Com.O2Bionics.ChatService.Web.Console.Models.ChatCustomization;
using Com.O2Bionics.ChatService.Contract.WidgetAppearance;
using Com.O2Bionics.ChatService.Web.Console.Models.AuditTrail;

namespace Com.O2Bionics.ChatService.Web.Console.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            if (User.IsInRole(RoleNames.Agent))
                return RedirectToAction("ChatSessions");
            if (User.IsInRole(RoleNames.Supervisor))
                return RedirectToAction("AgentStatistics");
            if (User.IsInRole(RoleNames.Admin))
                return RedirectToAction("ManageUsers");
            if (User.IsInRole(RoleNames.Owner))
                return RedirectToAction("ManageCustomer");
            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Owner)]
        public ActionResult ManageCustomer()
        {
// code for Customer Website (Admin)
// Edit Customer properties form (name?) (Owner)
// Disable Customer (Owner)
// Delete Customer (Owner)

            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Owner)]
        public ActionResult ManageCustomerFeatures()
        {
// list of features?
// addons?

            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public ActionResult ManageDepartments()
        {
// list of departments
// create new
// edit department properties subpage (or in-place)

            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public ActionResult ManageUsers()
        {
// create new
// list of users
// remove
// disable
// reset password
// edit user
// 	edit user properties
// 	set user roles
// 	set Owner role (Owner)
// 	set Admin role (Owner)
// 	set departments where user is Agent
// 	set departments where user is Supervisor

            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult> UserLoginLog()
        {
//	list of User logins with time and address
            var model = new AuditTrailModel
                {
                    Title = "Login log",
                    FormKind = "Login",
                    MaxDays = await FeatureServiceHelper.FetchVisibleDays(true, UserIdentity.GetCustomerId(User.Identity))
                };
            return View("AdminAuditTrail", model);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task<ActionResult> AdminAuditTrail()
        {
//	list of Admin actions with timestamp, admin user, action description
            var model = new AuditTrailModel
                {
                    Title = "Audit trail",
                    FormKind = "AuditTrail",
                    MaxDays = await FeatureServiceHelper.FetchVisibleDays(false, UserIdentity.GetCustomerId(User.Identity))
                };
            return View("AdminAuditTrail", model);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Supervisor + "," + RoleNames.Agent)]
        public ActionResult ChatSessionHistory()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Supervisor + "," + RoleNames.Agent)]
        public ActionResult ChatSessionDetails(uint sid)
        {
            return View(sid);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Supervisor)]
        public ActionResult AgentStatistics()
        {
//	view agent statistics reports for Superviser’s departments

            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Agent)]
        public ActionResult ChatSessions()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ManageUser()
        {
// manage own properties
// reset password

            return View();
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Owner)]
        public ActionResult InstallCode()
        {
            ViewBag.ChatJs = GetWidgetScript(UserIdentity.GetCustomerId(User.Identity));

            return View();
        }

        private string GetWidgetScript(uint customerId)
        {
            var scriptName = "widget-script.txt";

            var visitorChatUri = ChatServiceSettings.WidgetUrl.ToString();

            return System.IO.File.ReadAllText(Server.MapPath("~/" + scriptName))
                .Replace("%%customerId%%", customerId.ToString(CultureInfo.InvariantCulture))
                .Replace("%%domainUri%%", visitorChatUri);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Owner)]
        public ActionResult ChatWidgetAppearance()
        {
            var customerId = UserIdentity.GetCustomerId(User.Identity);
            var visitorChatUri = ChatServiceSettings.WidgetUrl.ToString().TrimEnd('/');

            var wai = ManagementService.Call(s => s.GetChatWidgetAppearanceInfo(customerId));
            if (null == wai) //TODO: p2. task-125. When customer is deleted - log off?
                return RedirectToAction(nameof(Index));

            var chatWidgetAppearance = wai.AppearanceData;
            var domains = string.IsNullOrWhiteSpace(wai.Domains) ? "o2bionics.com" : wai.Domains;
            var backgroundPageUrl = $"https://{domains.Split(';')[0]}/";

            var model = new ChatWidgetAppearanceViewModel
                {
                    ThemesUrl = $"{visitorChatUri}/themes/getlist",
                    WidgetScript = GetWidgetScript(customerId),
                    EnabledFeatures = wai.EnabledFeatures,
                    ThemeSelector = new ChatWidgetThemeSelector(wai.EnabledFeatures, chatWidgetAppearance),
                    ThemeMinSelector = new ChatWidgetMinThemeSelector(wai.EnabledFeatures, chatWidgetAppearance),
                    Positioning = new ChatWidgetPositioning(wai.EnabledFeatures, chatWidgetAppearance),
                    MinimizedState = new ChatWidgetMinimizedState(wai.EnabledFeatures, chatWidgetAppearance),
                    FullCustomization =
                        new ChatWidgetFullCSSCustomization(wai.EnabledFeatures, chatWidgetAppearance)
                            {
                                DownloadCssUrl = $"{visitorChatUri}/themes/maximized/{ChatWidgetThemes.Default}/styles.css"
                            },
                    PoweredBy = new ChatWidgetPoweredByVisible(wai.EnabledFeatures, chatWidgetAppearance),
                    BackgroundPageUrl = backgroundPageUrl,
                };

            return View(model);
        }

        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Owner)]
        [HttpPost]
        public ActionResult SaveChatWidgetAppearance(HashSet<string> enabledFeatures, ChatWidgetAppearance chatWidgetAppearance)
        {
            var customerId = UserIdentity.GetCustomerId(User.Identity);
            var userId = UserIdentity.GetUserId(User.Identity);

            var result = ManagementService.Call(s => s.SaveChatWidgetAppearanceJson(customerId, userId, chatWidgetAppearance, enabledFeatures));
            return Json(result);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Admin)]
        public ActionResult WidgetLoadStatistics()
        {
            return View();
        }

        private static TcpServiceClient<IManagementService> ManagementService =>
            GlobalContainer.Resolve<TcpServiceClient<IManagementService>>();

        private static WorkspaceSettings ChatServiceSettings =>
            GlobalContainer.Resolve<WorkspaceSettings>();
    }
}