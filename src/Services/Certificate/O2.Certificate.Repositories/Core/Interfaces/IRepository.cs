using System.Threading.Tasks;
using O2.Black.Toolkit.Core.Data;

namespace O2.Business.Repositories.Core.Interfaces
{
    public interface IRepository<TClass> 
        where TClass : class, IEntity
    {
        Task<TClass> ExistAsync<TType, TKey>(TKey typeValue, string nameProperty);
    }
}