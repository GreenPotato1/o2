using System.Threading.Tasks;
using PFRCenterGlobal.Core.Helpers;
using PFRCenterGlobal.Core.Services.RequestProvider;

namespace PFRCenterGlobal.Core.Services.Location
{
    public class LocationService : ILocationService
    {
        private readonly IRequestProvider _requestProvider;

        private const string ApiUrlBase = "api/v1/l/locations";

        public LocationService(IRequestProvider requestProvider)
        {
            _requestProvider = requestProvider;
        }

        public async Task UpdateUserLocation(Models.Location.Location newLocReq, string token)
        {
            var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayMarketingEndpoint, ApiUrlBase);

            await _requestProvider.PostAsync(uri, newLocReq, token);
        }
    }
}