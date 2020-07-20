using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils;
using Com.O2Bionics.Utils.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace Com.O2Bionics.ChatService.Web.Console
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            AntiForgeryConfig.RequireSsl = true;

            app.UseCookieAuthentication(
                new CookieAuthenticationOptions
                    {
                        AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                        LoginPath = new PathString(LoginConstants.LoginPath),
                        LogoutPath = new PathString(LoginConstants.LogoutPath),
                        CookieName = LoginConstants.CookieName,
                        CookiePath = "/",
                        CookieSecure = CookieSecureOption.Always,
                        CookieHttpOnly = true,
                        // TODO: add setting
                        ExpireTimeSpan = TimeSpan.FromDays(30),
                        SlidingExpiration = true,
                        Provider = new CookieAuthenticationProvider
                            {
                                OnValidateIdentity = OnValidateIdentity(
                                    // TODO: add setting
                                    TimeSpan.FromMinutes(1)),
//                                OnResponseSignOut = soCtx =>
//                                    {
//                                        m_log.Debug("sign out");
//                                        var t = soCtx.Request.Cookies["test"];
//                                        m_log.DebugFormat("sign out: {0} {1} {2}", soCtx.CookieOptions.Path , t ?? "(null)");
//
//                                    }
                            },
                    });
        }

        private static bool ShouldIgnoreRequest(IOwinContext context)
        {
            if (context.Request.Path.StartsWithSegments(new PathString("/st")))
                return true;
            if (context.Request.Path.StartsWithSegments(new PathString("/Errors")))
                return true;
            if (context.Request.Path == new PathString("/favicon.ico"))
                return true;
            return false;
        }

        private static Func<CookieValidateIdentityContext, Task> OnValidateIdentity(TimeSpan validateInterval)
        {
            return context => ShouldIgnoreRequest(context.OwinContext)
                ? Task.FromResult(0)
                : Task.Factory.StartNew(
                    () =>
                        {
                            var currentUtc = DateTimeOffset.UtcNow;
                            if (context.Options?.SystemClock != null)
                                currentUtc = context.Options.SystemClock.UtcNow;
                            var issuedUtc = context.Properties.IssuedUtc;
                            var validate = !issuedUtc.HasValue;
                            if (issuedUtc.HasValue)
                                validate = currentUtc.Subtract(issuedUtc.Value) > validateInterval;
                            if (!validate) return;

                            var userId = UserIdentity.GetUserId(context.Identity, false);
                            if (userId == 0) return;
                            var customerId = UserIdentity.GetCustomerId(context.Identity, false);
                            if (customerId == 0) return;

                            _log.DebugFormat("OnValidateIdentity: {0}", context.Request.Uri);
                            var service = GlobalContainer.Resolve<TcpServiceClient<IManagementService>>();
                            var user = service.Call(s => s.GetUserIdentity(customerId, userId));
                            if (user == null || user.Status != ObjectStatus.Active)
                            {
                                context.RejectIdentity();
                                context.OwinContext.Authentication.SignOut(context.Options.AuthenticationType);
                            }
                            else
                            {
                                context.Properties.IssuedUtc = new DateTimeOffset?();
                                context.Properties.ExpiresUtc = new DateTimeOffset?();
                                UserIdentity.SignIn(context.OwinContext, user, context.Identity);
                            }
                        });
        }
    }
}