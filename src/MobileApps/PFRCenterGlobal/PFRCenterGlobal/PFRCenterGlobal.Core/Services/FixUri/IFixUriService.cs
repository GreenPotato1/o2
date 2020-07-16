using System.Collections.Generic;
using PFRCenterGlobal.Core.Models.Basket;
using PFRCenterGlobal.Core.Models.Catalog;
using PFRCenterGlobal.Core.Models.Marketing;

namespace PFRCenterGlobal.Core.Services.FixUri
{
    public interface IFixUriService
    {
        void FixCatalogItemPictureUri(IEnumerable<CatalogItem> catalogItems);
        void FixBasketItemPictureUri(IEnumerable<BasketItem> basketItems);
        void FixCampaignItemPictureUri(IEnumerable<CampaignItem> campaignItems);
    }
}
