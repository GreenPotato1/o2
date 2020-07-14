using System.Threading.Tasks;
using System.Windows.Input;
using ArenaSApp.Models.Catalog;
using ArenaSApp.Services.Catalog;
using ArenaSApp.ViewModels.Base;
using Xamarin.Forms;

namespace ArenaSApp.ViewModels
{
    public class CatalogDetailViewModel: ViewModelBase
    {
        private ICatalogService _productsService;

        public CatalogDetailViewModel(ICatalogService productsService)
        {
            _productsService = productsService;
            Model = new CatalogItem();
        }
        public CatalogItem Model { get; set; }
        public ICommand SaveChangesCommand => new Command<CatalogItem>(async (CatalogItem obj) => await SaveItem());
        private async Task SaveItem()
        {
            IsBusy = true;
            MessagingCenter.Send(this, MessageKeys.AddProduct, Model);
            await _productsService.AddCatalogAsync(Model);
           
            await NavigationService.NavigateToAsync<MainViewModel>();
            await NavigationService.RemoveBackStackAsync();
            IsBusy = false;

        }
    }
}