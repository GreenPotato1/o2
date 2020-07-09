
using Microsoft.EntityFrameworkCore;

namespace O2.Black.Toolkit.Core
{
    public static class ExtensionsDataContext
    {
        public static DbSet<TModel> GetDataSet<TModel>(this DbContext dataContext) where TModel : class
        {
            return dataContext.Set<TModel>();
        }
    }
}