using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ArenaSApp.Models.Catalog;
using ArenaSApp.Services.Catalog;
using ArenaSApp.ViewModels.Base;

namespace ArenaSApp.ViewModels
{
    public class CatalogViewModel : ViewModelBase
    {
        private ObservableCollection<CatalogItem> _products;

        private ICatalogService _productsService;

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