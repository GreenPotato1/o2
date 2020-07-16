using System.Collections.Generic;
using PFRCenterGlobal.Core.Core.Models.Catalog;

namespace PFRCenterGlobal.Core.Core.Services.FixUri
{
    public interface IFixUriService
    {
        void FixCatalogItemPictureUri(IEnumerable<CatalogItem> catalogItems);
        // void FixBasketItemPictureUri(IEnumerable<BasketItem> basketItems);
        // void FixCampaignItemPictureUri(IEnumerable<CampaignItem> campaignItems);
    }
}
