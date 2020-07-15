using System.Collections.Generic;
using PFRCenterGlobal.Models.Catalog;

namespace PFRCenterGlobal.Services.FixUri
{
    public interface IFixUriService
    {
        void FixCatalogItemPictureUri(IEnumerable<CatalogItem> catalogItems);
        // void FixBasketItemPictureUri(IEnumerable<BasketItem> basketItems);
        // void FixCampaignItemPictureUri(IEnumerable<CampaignItem> campaignItems);
    }
}
