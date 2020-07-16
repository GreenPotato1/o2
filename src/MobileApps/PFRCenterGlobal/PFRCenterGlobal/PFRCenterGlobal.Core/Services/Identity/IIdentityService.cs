using System.Threading.Tasks;
using PFRCenterGlobal.Core.Models.Token;

namespace PFRCenterGlobal.Core.Services.Identity
{
    public interface IIdentityService
    {
        string CreateAuthorizationRequest();
        string CreateLogoutRequest(string token);
        Task<UserToken> GetTokenAsync(string code);
    }
}