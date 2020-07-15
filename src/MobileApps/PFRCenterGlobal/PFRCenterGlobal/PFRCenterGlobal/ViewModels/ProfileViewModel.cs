using System.Threading.Tasks;
using System.Windows.Input;
using PFRCenterGlobal.Models;
using PFRCenterGlobal.Models.User;
using PFRCenterGlobal.ViewModels.Base;
using Xamarin.Forms;

namespace PFRCenterGlobal.ViewModels
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