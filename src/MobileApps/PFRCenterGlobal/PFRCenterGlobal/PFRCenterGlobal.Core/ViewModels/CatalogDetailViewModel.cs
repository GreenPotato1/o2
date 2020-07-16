using System.Threading.Tasks;
using System.Windows.Input;
using PFRCenterGlobal.Core.Core.Models.Catalog;
using PFRCenterGlobal.Core.Core.Services.Catalog;
using PFRCenterGlobal.Core.Core.ViewModels.Base;
using Xamarin.Forms;

namespace PFRCenterGlobal.Core.Core.ViewModels
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