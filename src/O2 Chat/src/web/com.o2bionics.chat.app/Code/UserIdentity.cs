using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.Utils.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Com.O2Bionics.ChatService.Web.Console
{
    public class UserIdentity : ClaimsIdentity
    {
        private UserIdentity(IEnumerable<Claim> claims)
            : base(claims, DefaultAuthenticationTypes.ApplicationCookie)
        {
        }

        public static UserIdentity Create(UserInfo user, bool isPersistent)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Email),
                    new Claim(ClaimTypes.Sid, user.Id.ToString("#")),
                    new Claim(ClaimTypes.GroupSid, user.CustomerId.ToString("#")),
                    new Claim(ClaimTypes.Name, user.FullName()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimsIdentityExtensions.IsPersistentClaimType, isPersistent ? "1" : "0"),
                };

            if (user.AgentDepartments.Any()) claims.Add(new Claim(ClaimTypes.Role, RoleNames.Agent));
            if (user.SupervisorDepartments.Any()) claims.Add(new Claim(ClaimTypes.Role, RoleNames.Supervisor));
            if (user.IsAdmin) claims.Add(new Claim(ClaimTypes.Role, RoleNames.Admin));
            if (user.IsOwner) claims.Add(new Claim(ClaimTypes.Role, RoleNames.Owner));
            if (user.Avatar != null) claims.Add(new Claim(ClaimsIdentityExtensions.AvatarClaimType, user.Avatar));

            return new UserIdentity(claims);
        }

        public static bool GetIsPersistent(IIdentity identity)
        {
            return ((ClaimsIdentity)identity).GetIsPersistent();
        }

        public static string GetAvatar(IIdentity identity)
        {
            return ((ClaimsIdentity)identity).GetAvatar();
        }

        public static uint GetCustomerId(IIdentity identity, bool throwException = true)
        {
            return ((ClaimsIdentity)identity).GetCustomerId(throwException);
        }

        public static uint GetUserId(IIdentity identity, bool throwException = true)
        {
            return ((ClaimsIdentity)identity).GetUserId(throwException);
        }

        public static void SignIn(IOwinContext context, UserInfo user, IIdentity currentIdentity = null, bool? isPersistent = null)
        {
            var authenticationManager = context.Authentication;
            
            isPersistent = isPersistent ?? GetIsPersistent(currentIdentity);

            var identity = Create(user, isPersistent.Value);
            authenticationManager.SignIn(
                new AuthenticationProperties { IsPersistent = isPersistent.Value, AllowRefresh = true, },
                identity);
        }
    }
}