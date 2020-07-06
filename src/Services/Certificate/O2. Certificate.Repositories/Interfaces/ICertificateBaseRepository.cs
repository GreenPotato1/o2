using System.Collections.Generic;
using System.Threading.Tasks;
using O2.Black.Toolkit.Core;
using O2.Black.Toolkit.Core.Data;
using O2.Certificate.Data.Models.O2C;
using O2.Certificate.Repositories.Core.Interfaces;
using O2.Certificate.Repositories.Helper;

namespace O2.Certificate.Repositories.Interfaces
{
    public interface ICertificateBaseRepository<TClass>  : IBaseRepository<TClass> 
        where TClass: class, IEntity
    {
        Task<IEnumerable<TClass>> GetAllAsync(bool showAll);
        Task<TClass> LoadPhoto(TClass existEvent, O2CPhoto o2CPhoto);
        Task<List<TClass>> AddRangeAsync(List<TClass> listEntities, bool cleanData);
        Task<PagedList<TClass>> GetAllAsync(CertificateParam certificateParam, bool info);
    }
}