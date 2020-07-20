using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Web;
using JetBrains.Annotations;
using Microsoft.AspNet.Identity;

namespace Com.O2Bionics.Utils.Web
{
    public static class ClaimsIdentityExtensions
    {
        public const string AvatarClaimType = "Avatar";
        public const string IsPersistentClaimType = "p";

        public static bool GetIsPersistent([NotNull] this ClaimsIdentity identity)
        {
            return identity.FindFirstValue(IsPersistentClaimType) == "1";
        }

        public static string GetAvatar([NotNull] this ClaimsIdentity identity)
        {
            return identity.FindFirstValue(AvatarClaimType);
        }

        public static uint GetCustomerId([NotNull] this ClaimsIdentity identity, bool throwException = true)
        {
            return GetUint(identity, ClaimTypes.GroupSid, throwException);
        }

        public static uint GetUserId([NotNull] this ClaimsIdentity identity, bool throwException = true)
        {
            return GetUint(identity, ClaimTypes.Sid, throwException);
        }

        private static uint GetUint([NotNull] this ClaimsIdentity identity, [NotNull] string claimType, bool throwException)
        {
            var valueString = identity.FindFirstValue(claimType);
            if (!string.IsNullOrEmpty(valueString) &&
                uint.TryParse(valueString, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            if (throwException)
                throw new HttpException((int)HttpStatusCode.BadRequest, "Invalid " + claimType);
            return 0u;
        }
    }
}