#if ERRORTRACKERTEST
using System;
using System.Runtime.CompilerServices;
using System.Web.Mvc;
using Com.O2Bionics.ChatService.Contract;

namespace Com.O2Bionics.ChatService.Web.Console.Controllers
{
    [AllowAnonymous]
    public class TestController : Controller
    {
        [HttpGet]
        public void TestWcf()
        {
            Response.StatusCode = ErrorTrackerTestHelper.RunTest<IManagementService>(out var message);
            Response.Write(message);
        }

        [HttpGet]
        public void TestAction()
        {
            ThrowException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowException()
        {
            throw new Exception($"{nameof(TestController)}.{nameof(TestAction)} passed at {DateTime.UtcNow} - check the Elastic server.");
        }
    }
}
#endif