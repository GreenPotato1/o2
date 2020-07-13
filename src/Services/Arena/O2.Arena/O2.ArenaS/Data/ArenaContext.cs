using Microsoft.EntityFrameworkCore;

namespace O2.ArenaS.Data
{
    public class ArenaContext: DbContext
    {
        private readonly DbContextOptions<ArenaContext> _options;

        #region Ctors

        public ArenaContext(DbContextOptions<ArenaContext> options)
            : base(options)
        {
            _options = options;
        }

        #endregion

        #region Fields
        public DbSet<Item> Items { get; set; }
        #endregion
    }
}