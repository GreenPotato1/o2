using System.Threading.Tasks;
using PFRCenterGlobal.Models.Token;

namespace PFRCenterGlobal.Services.Identity
{
    public interface IIdentityService
    {
        string CreateAuthorizationRequest();
        string CreateLogoutRequest(string token);
        Task<UserToken> GetTokenAsync(string code);
    }
}