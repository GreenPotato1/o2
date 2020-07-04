using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using O2.Black.Toolkit.Core;
using O2.Black.Toolkit.Core.Data;
using O2.Business.Data;
using O2.Business.Repositories.Core.Interfaces;

namespace O2.Business.Repositories.Core
{
    public abstract class BaseRepository<TClass> : Repository<TClass>,
        IBaseRepository<TClass>
        where TClass : class, IEntity
    {

        #region Ctors

        protected BaseRepository(O2BusinessDataContext context) 
            : base(context)
        {
        }

        #endregion

        public virtual async Task<IEnumerable<TClass>> GetAllAsync()
        {
            var itemType = await DataContext.GetDataSet<TClass>().ToListAsync();
            return itemType;
        }
        
        public virtual async Task<TClass> GetAsync(Guid id)
        {
            return await DataContext.GetDataSet<TClass>().FirstOrDefaultAsync(entity => entity.Id == id);
        }

        public async Task<TClass> AddOrUpdateAsync(TClass entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            
            if (entity.Id == Guid.Empty) 
                return await AddBaseAsync(entity);

            var exist = await GetAsync(entity.Id);
            if (exist == null)
            {
                entity.CreateEntity();
                return await AddBaseAsync(entity);
            }
            else
            {
                // DeepCloneExt.Copy(entity,exist);
                CloneForUpdate(entity, exist);
                return await UpdateAsync(exist);
            }
        }

        public abstract void CloneForUpdate(TClass sourceEntity, TClass exist);
       

        public virtual async Task<TClass> AddBaseAsync(TClass entity)
        {
            try
            {
                var result = await DataContext.AddAsync(entity);
                await SaveAllAsync();
                return result.Entity;
            }
            catch (DbUpdateException e)
            {
                Console.WriteLine(e);
                throw;
            }
      
        }

        public async Task<List<TClass>> AddRangeAsync(List<TClass> listEntities)
        {
            foreach (var item in listEntities)
            {
                await AddOrUpdateAsync(item);
            }
            return listEntities;
        }

        public virtual async Task<TClass> UpdateAsync(TClass entity)
        {
            entity.UpdateEntity();
            // DataContext.GetDataSet<TClass>().Attach(entity);
            DataContext.Entry(entity).State = EntityState.Modified;
            await SaveAllAsync();
            return entity;
        }

        public async Task<bool> SaveAllAsync()
        {
            return await DataContext.SaveChangesAsync() > 0;
        }

        public async Task<TClass> DeleteAsync(Guid byId)
        {
            var exist = await GetAsync(byId);
            if (exist != null)
            {
                DataContext.Entry(exist).State = EntityState.Deleted;
                await SaveAllAsync();
                return exist;
            }
            else
            {
                throw new Exception("Object not found");
            }
        }
    }
}