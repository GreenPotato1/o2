using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using O2.ArenaS.Data;

namespace O2.ArenaS.Services
{
    public interface ICatalogItemService
    {
        Task<IReadOnlyCollection<CatalogItem>> GetAllAsync(CancellationToken ct);

        Task<CatalogItem> GetByIdAsync(int id, CancellationToken ct);

        Task<CatalogItem> UpdateAsync(CatalogItem catalogItem, CancellationToken ct);

        Task<CatalogItem> AddAsync(CatalogItem catalogItem, CancellationToken ct);

        Task RemoveAsync(int id, CancellationToken ct);
    }
}