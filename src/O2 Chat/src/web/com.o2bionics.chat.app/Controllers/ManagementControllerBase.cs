using System.Web.Mvc;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Web;

namespace Com.O2Bionics.ChatService.Web.Console.Controllers
{
    public class ManagementControllerBase : Controller
    {
        protected uint CustomerId => 
            UserIdentity.GetCustomerId(User.Identity);

        protected uint CurrentUserId =>
            UserIdentity.GetUserId(User.Identity);

        protected static TcpServiceClient<IManagementService> ManagementService =>
            GlobalContainer.Resolve<TcpServiceClient<IManagementService>>();

        protected static TcpServiceClient<IAgentConsoleService> AgentService =>
            GlobalContainer.Resolve<TcpServiceClient<IAgentConsoleService>>();

        protected static JsonResult JilJson(object result)
        {
            return new JilJsonResult
                {
                    Data = result,
                };
        }
        
        protected static JsonResult VoidJson => JilJson(null);

        protected void Write(int statusCode, string message = null)
        {
            Response.StatusCode = statusCode;
            if (!string.IsNullOrEmpty(message))
                Response.Write(message);
        }
    }
}