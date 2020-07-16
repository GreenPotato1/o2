using System.Threading.Tasks;

namespace PFRCenterGlobal.Core.Services.Location
{    
    public interface ILocationService
    {
        Task UpdateUserLocation(Models.Location.Location newLocReq, string token);
    }
}