using System.Threading.Tasks;
using System.Windows.Input;
using ArenaSApp.Models;
using ArenaSApp.ViewModels.Base;
using Xamarin.Forms;

namespace ArenaSApp.ViewModels
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