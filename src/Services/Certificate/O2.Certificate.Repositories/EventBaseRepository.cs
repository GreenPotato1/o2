using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using O2.Black.Toolkit.Core;
using O2.Business.Repositories.Core;
using O2.Business.Repositories.Interfaces;
using O2.Certificate.Data;
using O2.Certificate.Data.Models.O2Ev;

namespace O2.Business.Repositories
{
    public class EventBaseRepository<TClass> :
        BaseRepository<TClass>,
        IEventBaseRepository<TClass>
        where TClass : O2EvEvent
    {
        #region Ctors

        public EventBaseRepository(O2BusinessDataContext context) :
            base(context)
        {
        }

        #endregion

        public override async Task<TClass> GetAsync(Guid id)
        {
            return await DataContext.GetDataSet<TClass>() .Include(p => p.Meta)
                .Include(p => p.Photos).FirstOrDefaultAsync(entity => entity.Id == id);
        }

        public override void CloneForUpdate(TClass sourceEntity, TClass exist)
        {
            DeepCloneExt.Copy(sourceEntity,exist);
        }

        public override async Task<IEnumerable<TClass>> GetAllAsync()
        {
            var itemType = await DataContext.GetDataSet<TClass>()
                .Include(p => p.Meta)
                .Include(p => p.Photos)
                .OrderBy(x => x.StartDate)
                .ToListAsync();

            return itemType;
        }

        public async Task<IEnumerable<TClass>> GetAllAsync(bool info, bool last, int countLast)
        {
            if (!info)
               return await GetAllAsync();
            
            var itemType = await DataContext.GetDataSet<TClass>()
                .Include(p => p.Meta)
                .Include(p => p.Photos)
                .OrderBy(x => x.StartDate)
                .ToListAsync();

            if (last)
            {
                var listResult = new List<TClass>();
                for (int i = 0; i < countLast; i++)
                {
                    if(itemType.Count-i !=0)
                     listResult.Add(itemType.ElementAt(itemType.Count-i));
                }

                return listResult;
            }
            return itemType.Where(item => item.EndDate >= DateTime.Now.ConvertToUnixTime()).ToList();
        }

        public async Task<List<TClass>> AddRangeAsync(List<TClass> listEntities, bool cleanData)
        {
            // DataContext.GetDataSet<TClass>().Clear();
            var all = await  GetAllAsync();
            foreach (var getEntity in all )
            {
                DataContext.Entry(getEntity).State = EntityState.Deleted;
            }
            DataContext.SaveChanges();
            return await base.AddRangeAsync(listEntities);
        }

        public async Task<TClass> LoadPhoto(TClass existEvent, O2EvPhoto o2EvPhoto)
        {
            DataContext.Attach(o2EvPhoto);
            o2EvPhoto.O2EvEvent = existEvent;
            existEvent.Photos.Add(o2EvPhoto);
            DataContext.O2EvPhoto.Add(o2EvPhoto);
            return await AddOrUpdateAsync(existEvent);
        }
    }
}