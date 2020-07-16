using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PFRCenterGlobal.Core.Models.Marketing;

namespace PFRCenterGlobal.Core.Services.Marketing
{
    public interface ICampaignService
    {
        Task<ObservableCollection<CampaignItem>> GetAllCampaignsAsync(string token);
        Task<CampaignItem> GetCampaignByIdAsync(int id, string token);
    }
}