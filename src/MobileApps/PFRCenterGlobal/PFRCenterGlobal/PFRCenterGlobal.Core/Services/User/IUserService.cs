using System.Threading.Tasks;
using PFRCenterGlobal.Core.Models.User;

namespace PFRCenterGlobal.Core.Services.User
{
    public interface IUserService
    {
        Task<UserInfo> GetUserInfoAsync(string authToken);
    }
}
