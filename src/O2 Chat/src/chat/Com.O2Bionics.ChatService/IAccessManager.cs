using Com.O2Bionics.ChatService.Objects;

namespace Com.O2Bionics.ChatService
{
    public interface IAccessManager
    {
        void CheckUserStatus(User user, uint customerId, uint userId);
        void CheckUserHasRoles(User user, params UserRole[] requiredRoles);
    }
}