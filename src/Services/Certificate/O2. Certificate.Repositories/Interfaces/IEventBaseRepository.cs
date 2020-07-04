using System.Collections.Generic;
using System.Threading.Tasks;
using O2.Black.Toolkit.Core.Data;
using O2.Business.Data.Models.O2Ev;
using O2.Business.Repositories.Core.Interfaces;

namespace O2.Business.Repositories.Interfaces
{
    public interface IEventBaseRepository<TClass> : IBaseRepository<TClass> 
        where TClass: class, IEntity
    {
        Task<IEnumerable<TClass>> GetAllAsync(bool info, bool last, int countLast);
        Task<List<TClass>> AddRangeAsync(List<TClass> listEntities, bool cleanData);
        Task<TClass> LoadPhoto(TClass existEvent, O2EvPhoto o2EvPhoto);
    }
}