using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ArenaSApp.Helpers;
using ArenaSApp.Models.Catalog;
using ArenaSApp.Services.FixUri;
using ArenaSApp.Services.RequestProvider;

namespace ArenaSApp.Services.Catalog
{
    public class CatalogService : ICatalogService
    {
        private readonly IRequestProvider _requestProvider;
        private readonly IFixUriService _fixUriService;

        private const string ApiUrlBase = "api/v1.0/items";

        public CatalogService(IRequestProvider requestProvider, IFixUriService fixUriService)
        {
            _requestProvider = requestProvider;
            _fixUriService = fixUriService;
        }

        //public async Task<ObservableCollection<CatalogItem>> FilterAsync(int catalogBrandId, int catalogTypeId)
        //{
        //    var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayShoppingEndpoint, $"{ApiUrlBase}/items/type/{catalogTypeId}/brand/{catalogBrandId}");

        //    CatalogRoot catalog = await _requestProvider.GetAsync<CatalogRoot>(uri);

        //    if (catalog?.Data != null)
        //        return catalog?.Data.ToObservableCollection();
        //    else
        //        return new ObservableCollection<CatalogItem>();
        //}


        //public async Task<ObservableCollection<CatalogBrand>> GetCatalogBrandAsync()
        //{
        //    var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayShoppingEndpoint, $"{ApiUrlBase}/catalogbrands");

        //    IEnumerable<CatalogBrand> brands = await _requestProvider.GetAsync<IEnumerable<CatalogBrand>>(uri);

        //    if (brands != null)
        //        return brands?.ToObservableCollection();
        //    else
        //        return new ObservableCollection<CatalogBrand>();
        //}

        //public async Task<ObservableCollection<CatalogType>> GetCatalogTypeAsync()
        //{
        //    var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayShoppingEndpoint, $"{ApiUrlBase}/catalogtypes");

        //    IEnumerable<CatalogType> types = await _requestProvider.GetAsync<IEnumerable<CatalogType>>(uri);

        //    if (types != null)
        //        return types.ToObservableCollection();
        //    else
        //        return new ObservableCollection<CatalogType>();
        //}


        public async Task<ObservableCollection<CatalogItem>> GetCatalogAsync()
        {
            var uri = UriHelper.CombineUri(GlobalSetting.Instance.GatewayShoppingEndpoint, $"{ApiUrlBase}/items");

            IEnumerable < CatalogItem> catalog = await _requestProvider.GetAsync< IEnumerable < CatalogItem >>(uri);

            if (catalog != null)
                return catalog.ToObservableCollection();
            else
                return new ObservableCollection<CatalogItem>();
        }



        public Task AddCatalogAsync(CatalogItem catalogItem)
        {
            throw new NotImplementedException();
        }

        public Task DeleteCatalogAsync(CatalogItem catalogItem)
        {
            throw new NotImplementedException();
        }

        public Task Update(CatalogItem model)
        {
            throw new NotImplementedException();
        }

        public Task<CatalogItem> GetProductByIdAsync(int navigationData)
        {
            throw new NotImplementedException();
        }
    }

    public static class ObservableExtension
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            ObservableCollection<T> collection = new ObservableCollection<T>();

            foreach (T item in source)
            {
                collection.Add(item);
            }

            return collection;
        }
    }
}
