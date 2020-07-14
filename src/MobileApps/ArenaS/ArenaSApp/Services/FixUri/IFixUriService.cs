using System.Collections.Generic;
using ArenaSApp.Models.Catalog;

namespace ArenaSApp.Services.FixUri
{
    public interface IFixUriService
    {
        void FixCatalogItemPictureUri(IEnumerable<CatalogItem> catalogItems);
        // void FixBasketItemPictureUri(IEnumerable<BasketItem> basketItems);
        // void FixCampaignItemPictureUri(IEnumerable<CampaignItem> campaignItems);
    }
}
