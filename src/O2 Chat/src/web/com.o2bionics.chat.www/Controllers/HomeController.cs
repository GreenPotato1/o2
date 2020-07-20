using System.Web.Mvc;

namespace Com.O2Bionics.Chat.Web.Controllers
{
    public class HomeController : Controller
    {
        [Route("~/", Name = "default")]
        [Route("index")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("features")]
        public ActionResult Features()
        {
            return View();
        }

        [Route("contact")]
        public ActionResult Contact()
        {
            return View();
        }

        [Route("pricing")]
        public ActionResult Pricing()
        {
            return View();
        }
        
        [Route("about")]
        public ActionResult About()
        {
            return View();
        }

        public ActionResult PageNotFound()
        {
            return View();
        }
    }
}