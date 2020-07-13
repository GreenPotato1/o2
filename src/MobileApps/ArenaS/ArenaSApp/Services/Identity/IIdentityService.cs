using System.Threading.Tasks;
using ArenaSApp.Models.Token;

namespace ArenaSApp.Services.Identity
{
    public interface IIdentityService
    {
        string CreateAuthorizationRequest();
        string CreateLogoutRequest(string token);
        Task<UserToken> GetTokenAsync(string code);
    }
}