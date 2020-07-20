using Com.O2Bionics.ChatService.Contract;
using Com.O2Bionics.ChatService.Objects;
using Com.O2Bionics.ChatService.Properties;

namespace Com.O2Bionics.ChatService.Impl
{
    public class AccessManager : IAccessManager
    {
        public void CheckUserStatus(User user, uint customerId, uint userId)
        {
            if (user == null || user.Status == ObjectStatus.Deleted)
            {
                throw new CallResultException(
                    CallResultStatusCode.AccessDenied,
                    new ValidationMessage("userId", string.Format(Resources.UserNotFoundError1, userId)));
            }

            if (user.Status == ObjectStatus.Disabled)
            {
                throw new CallResultException(
                    CallResultStatusCode.AccessDenied,
                    new ValidationMessage("userId", "User is disabled, id=" + userId));
            }

            if (user.Status == ObjectStatus.NotConfirmed)
            {
                throw new CallResultException(
                    CallResultStatusCode.AccessDenied,
                    new ValidationMessage("userId", "User's email is not confirmed, id=" + userId));
            }

            if (user.CustomerId != customerId)
            {
                throw new CallResultException(
                    CallResultStatusCode.AccessDenied,
                    new ValidationMessage("userId", "User\'s Customer is different from required"));
            }
        }

        public void CheckUserHasRoles(User user, params UserRole[] requiredRoles)
        {
            foreach (var requiredRole in requiredRoles)
            {
                if (!user.IsInRole(requiredRole))
                {
                    throw new CallResultException(
                        CallResultStatusCode.AccessDenied,
                        new ValidationMessage("userId", $"User is not in the {requiredRole} role"));
                }
            }
        }
    }
}