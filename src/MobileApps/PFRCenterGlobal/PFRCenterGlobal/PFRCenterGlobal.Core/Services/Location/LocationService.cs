using System.Threading.Tasks;
using PFRCenterGlobal.Core.Core;
using PFRCenterGlobal.Core.Core.Helpers;
using PFRCenterGlobal.Core.Core.Services.Location;
using PFRCenterGlobal.Core.Core.Services.RequestProvider;

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

        public async Task UpdateUserLocation(Core.Models.Location.Location newLocReq, string token)
        {
            var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayMarketingEndpoint, ApiUrlBase);

            await _requestProvider.PostAsync(uri, newLocReq, token);
        }
    }
}