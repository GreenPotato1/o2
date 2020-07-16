using System.Threading.Tasks;
using PFRCenterGlobal.Core.Helpers;
using PFRCenterGlobal.Core.Models.User;
using PFRCenterGlobal.Core.Services.RequestProvider;

namespace PFRCenterGlobal.Core.Services.User
{
    public class UserService : IUserService
    {
        private readonly IRequestProvider _requestProvider;

        public UserService(IRequestProvider requestProvider)
        {
            _requestProvider = requestProvider;
        }

        public async Task<UserInfo> GetUserInfoAsync(string authToken)
        {
            var uri = UriHelper.CombineUri(GlobalSetting.Instance.UserInfoEndpoint);

            var userInfo = await _requestProvider.GetAsync<UserInfo>(uri, authToken);
            return userInfo;
        }
    }
}