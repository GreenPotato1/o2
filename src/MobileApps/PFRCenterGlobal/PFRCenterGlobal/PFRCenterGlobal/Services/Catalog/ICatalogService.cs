using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PFRCenterGlobal.Models.Catalog;

namespace PFRCenterGlobal.Services.Catalog
{
    public interface ICatalogService
    {
        //Task<ObservableCollection<CatalogBrand>> GetCatalogBrandAsync();
        //Task<ObservableCollection<CatalogItem>> FilterAsync(int catalogBrandId, int catalogTypeId);
        //Task<ObservableCollection<CatalogType>> GetCatalogTypeAsync();
        Task<ObservableCollection<CatalogItem>> GetCatalogAsync();
        Task AddCatalogAsync(CatalogItem catalogItem);
        Task DeleteCatalogAsync(CatalogItem catalogItem);
        Task Update(CatalogItem model);
        Task<CatalogItem> GetProductByIdAsync(int navigationData);
    }
}
