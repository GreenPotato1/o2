using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using PFRCenterGlobal.Core.Core.Models.Catalog;
using PFRCenterGlobal.Core.Core.Services.Catalog;
using PFRCenterGlobal.Core.Core.ViewModels.Base;
using Xamarin.Forms;

namespace PFRCenterGlobal.Core.Core.ViewModels
{
    public class CatalogViewModel : ViewModelBase
    {
        private ObservableCollection<CatalogItem> _products;

        private ICatalogService _productsService;

        //AddItemCommmand
        public ICommand AddCatalogItemCommand => new Command<CatalogItem>(async (CatalogItem obj) => await AddCatalogItem(new CatalogItem()));
        public ICommand  OpenItemCommand => new Command<CatalogItem>(async (CatalogItem obj) => await OpenCatalogItem(obj));

        private async Task OpenCatalogItem(CatalogItem catalogItem)
        {
            IsBusy = true;
            // Add new item to Basket
            MessagingCenter.Send(this, MessageKeys.AddProduct, catalogItem);
            await NavigationService.NavigateToAsync<CatalogEditViewModel>(catalogItem.Id);
            IsBusy = false;
        }

        private async Task AddCatalogItem(CatalogItem catalogItem)
        {
            IsBusy = true;
            // Add new item to Basket
            MessagingCenter.Send(this, MessageKeys.AddProduct, catalogItem);
            await NavigationService.NavigateToAsync<CatalogDetailViewModel>();
            IsBusy = false;
        }
        
        public CatalogViewModel(ICatalogService productsService)
        {
            _productsService = productsService;
        }

        public ObservableCollection<CatalogItem> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                RaisePropertyChanged(() => Products);
            }
        }

        public override async Task InitializeAsync(object navigationData)
        {
            IsBusy = true;

            // Get Catalog, Brands and Types
            Products = await _productsService.GetCatalogAsync();
            //Brands = await _productsService.GetCatalogBrandAsync();
            //Types = await _productsService.GetCatalogTypeAsync();

            IsBusy = false;
        }
    }
}