using System.Threading.Tasks;
using System.Windows.Input;
using PFRCenterGlobal.Core.Core.Models.User;
using PFRCenterGlobal.Core.Core.ViewModels.Base;
using Xamarin.Forms;

namespace PFRCenterGlobal.Core.Core.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {

        public ICommand LogoutCommand => new Command(async () => await LogoutAsync());

        private async Task LogoutAsync()
        {
            IsBusy = true;

            // Logout
            await NavigationService.NavigateToAsync<LoginViewModel>(new LogoutParameter { Logout = true });
            await NavigationService.RemoveBackStackAsync();

            IsBusy = false;
        }
    }
}