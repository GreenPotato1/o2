using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using O2.Black.Toolkit.Core;
using O2.Black.Toolkit.Core.Data;
using O2.Certificate.Data;
using O2.Certificate.Repositories.Core.Interfaces;

namespace O2.Certificate.Repositories.Core
{
    public abstract class Repository<TClass> : IRepository<TClass>
        where TClass : class, IEntity
    {
        #region Fields

        protected readonly O2BusinessDataContext DataContext;
         
        #endregion


        #region Ctors

        protected Repository(O2BusinessDataContext context)
        {
            DataContext = context;
        }

        #endregion


        #region Methods

        public async Task<TClass> ExistAsync<TType, TKey>(TKey typeValue, string nameProperty)
        {
            if (DataContext.GetDataSet<TClass>().CountAsync().GetAwaiter().GetResult() == 0)
                    return null;
            TClass result = null;
            var entities = await DataContext.GetDataSet<TClass>().ToListAsync();
            foreach (var entity in entities)
            {
                if (entity.GetType().GetProperty(nameProperty)?.GetValue(entity, null)?.ToString() ==
                    typeValue.ToString())
                {
                    result = (TClass) entity;
                }
            }

            if (result != null)
                return await Task.FromResult<TClass>(result);
            return null;
        }

        #endregion
    }
}