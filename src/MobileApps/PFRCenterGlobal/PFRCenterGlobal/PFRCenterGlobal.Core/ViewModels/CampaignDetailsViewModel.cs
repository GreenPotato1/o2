using System.Threading.Tasks;
using System.Windows.Input;
using PFRCenterGlobal.Core.Models.Marketing;
using PFRCenterGlobal.Core.Services.Marketing;
using PFRCenterGlobal.Core.Services.Settings;
using PFRCenterGlobal.Core.ViewModels.Base;
using Xamarin.Forms;

namespace PFRCenterGlobal.Core.ViewModels
{
    public class CampaignDetailsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ICampaignService _campaignService;

        private CampaignItem _campaign;
        private bool _isDetailsSite;

        public ICommand EnableDetailsSiteCommand => new Command(EnableDetailsSite);

        public CampaignDetailsViewModel(ISettingsService settingsService, ICampaignService campaignService)
        {
            _settingsService = settingsService;
            _campaignService = campaignService;
        }

        public CampaignItem Campaign
        {
            get => _campaign;
            set
            {
                _campaign = value;
                RaisePropertyChanged(() => Campaign);
            }
        }

        public bool IsDetailsSite
        {
            get => _isDetailsSite;
            set
            {
                _isDetailsSite = value;
                RaisePropertyChanged(() => IsDetailsSite);
            }
        }

        public override async Task InitializeAsync(object navigationData)
        {
            if (navigationData is int)
            {
                IsBusy = true;
                // Get campaign by id
                Campaign = await _campaignService.GetCampaignByIdAsync((int)navigationData, _settingsService.AuthAccessToken);
                IsBusy = false;
            }
        }

        private void EnableDetailsSite()
        {
            IsDetailsSite = true;
        }
    }
}