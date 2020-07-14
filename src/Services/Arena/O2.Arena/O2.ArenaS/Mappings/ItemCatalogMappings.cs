using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using O2.ArenaS.Data;
using O2.ArenaS.DTOs;

namespace O2.ArenaS.Mappings
{
    public static class ItemCatalogMappings
    {
        public static CatalogItemViewModel ToViewModel(this CatalogItem model)
        {
            return model != null ? new CatalogItemViewModel()
            {
                Id = model.Id,
                Category = model.Category,
                Position = model.Position,
                Room = model.Room,
                RoomDescription = model.RoomDescription,
                RoomNumber = model.RoomNumber,
                SpecialNumber = model.SpecialNumber,
                KeyCount = model.KeyCount,
                Note = model.Note,
                RootType = model.RootType
            } : null;
        }

        public static CatalogItem ToServiceModel(this CatalogItemViewModel model)
        {
            return model != null ? new CatalogItem()
            {
                Id = model.Id,
                Category = model.Category,
                Position = model.Position,
                Room = model.Room,
                RoomDescription = model.RoomDescription,
                RoomNumber = model.RoomNumber,
                SpecialNumber = model.SpecialNumber,
                KeyCount = model.KeyCount,
                Note = model.Note,
                RootType = model.RootType
            } : null;
        }

        public static IReadOnlyCollection<CatalogItemViewModel> ToViewModel(this IReadOnlyCollection<CatalogItem> models)
        {
            if (models.Count == 0)
                return Array.Empty<CatalogItemViewModel>();
            
            var itemViewModels = new CatalogItemViewModel[models.Count];
            var i = 0;
            foreach (var model in models)
            {
                itemViewModels[i] = model.ToViewModel();
                ++i;
            }
            return new ReadOnlyCollection<CatalogItemViewModel>(itemViewModels);
        }
    }
}
