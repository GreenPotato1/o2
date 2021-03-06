using Microsoft.EntityFrameworkCore;

namespace O2.Business.Repositories
{
    public static class  ExtensionsDbSet
    {
        public static void Clear<T>(this DbSet<T> dbSet) where T : class
        {
            dbSet.RemoveRange(dbSet);
        }
    }
}