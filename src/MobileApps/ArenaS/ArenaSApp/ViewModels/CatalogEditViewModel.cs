using System.Threading.Tasks;
using System.Windows.Input;
using ArenaSApp.Models.Catalog;
using ArenaSApp.Services.Catalog;
using ArenaSApp.ViewModels.Base;
using Xamarin.Forms;

namespace ArenaSApp.ViewModels
{
    public class CatalogEditViewModel: ViewModelBase
    {
        private ICatalogService _productsService;
        
        private CatalogItem _model;

        public CatalogItem Model
        {
            
            get
            {
                return _model;
            }
            set
            {
                _model = value;
                RaisePropertyChanged(() => Model);
            }
        }
        
        public override async Task InitializeAsync(object navigationData)
        {
            if (navigationData is int)
            {
                IsBusy = true;
                // Get campaign by id
                Model = await _productsService.GetProductByIdAsync((int)navigationData);
                IsBusy = false;
            }
        }
        
        public CatalogEditViewModel(ICatalogService productsService)
        {
            _productsService = productsService;
            Model = new CatalogItem();
        }
        
        public ICommand  DeleteCatalogItemCommand => new Command(async () => await DeleteCatalogItem());

        public ICommand  SaveChangesCommand => new Command<CatalogItem>(async (CatalogItem obj) => await SaveChangesCatalogItem());
        
        private async Task OpenCatalogItem(CatalogItem catalogItem)
        {
            IsBusy = true;
            MessagingCenter.Send(this, MessageKeys.AddProduct, catalogItem);
            
            await NavigationService.NavigateToAsync<MainViewModel>();
            IsBusy = false;


        }
        private async Task SaveChangesCatalogItem()
        {
            IsBusy = true;
            // Add new item to Basket
            MessagingCenter.Send(this, MessageKeys.AddProduct, Model);
            await _productsService.Update(Model);
            
            await NavigationService.NavigateToAsync<MainViewModel>();
            IsBusy = false;

        }
        
        private async Task DeleteCatalogItem()
        {
            IsBusy = true;
            // Add new item to Basket
            MessagingCenter.Send(this, MessageKeys.AddProduct, Model);
            await _productsService.DeleteCatalogAsync(Model);
            
            await NavigationService.NavigateToAsync<MainViewModel>();
            IsBusy = false;
        }
    }
}