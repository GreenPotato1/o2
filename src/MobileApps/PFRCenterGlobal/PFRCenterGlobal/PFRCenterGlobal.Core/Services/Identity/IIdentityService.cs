using System.Threading.Tasks;
using PFRCenterGlobal.Core.Core.Models.Token;

namespace PFRCenterGlobal.Core.Core.Services.Identity
{
    public interface IIdentityService
    {
        string CreateAuthorizationRequest();
        string CreateLogoutRequest(string token);
        Task<UserToken> GetTokenAsync(string code);
    }
}