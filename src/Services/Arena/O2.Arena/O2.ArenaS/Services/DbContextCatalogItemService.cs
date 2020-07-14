using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using O2.ArenaS.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace O2.ArenaS.Services
{
    public class DbContextCatalogItemService:ICatalogItemService
    {
        private readonly ArenaContext _arenaContext;
        public DbContextCatalogItemService(ArenaContext arenaContext)
        {
            _arenaContext = arenaContext;
        }

        public Task<IReadOnlyCollection<CatalogItem>> GetAllAsync(CancellationToken ct)
        {
            var items = _arenaContext.Items;
            return Task.FromResult<IReadOnlyCollection<CatalogItem>>(_arenaContext.Items.ToList().AsReadOnly());
        }

        public  Task<CatalogItem> GetByIdAsync(int id, CancellationToken ct)
        {
            return _arenaContext.Items.SingleOrDefaultAsync(x => x.Id == id, cancellationToken: ct);
        }

        public Task<CatalogItem> UpdateAsync(CatalogItem catalogItem, CancellationToken ct)
        {
            //var item = _arenaContext.Items.SingleOrDefaultAsync(x => x.Id == catalogItem.Id, ct).GetAwaiter().GetResult();
            _arenaContext.Update(catalogItem);
            _arenaContext.SaveChanges();
            return Task.FromResult(catalogItem);
        }

        public Task<CatalogItem> AddAsync(CatalogItem catalogItem, CancellationToken ct)
        {
            _arenaContext.Items.Add(catalogItem);
            _arenaContext.SaveChanges();
            return Task.FromResult(catalogItem);
        }

        public Task RemoveAsync(int id, CancellationToken ct)
        {
            CatalogItem item = _arenaContext.Items.SingleOrDefaultAsync(x => x.Id == id, ct).GetAwaiter().GetResult();
            _arenaContext.Items.Remove(item);
            _arenaContext.SaveChanges();
            return Task.FromResult(item);
        }
    }
}