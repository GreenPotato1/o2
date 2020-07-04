using Microsoft.EntityFrameworkCore;
using O2.Business.Data.Models.O2C;
using O2.Business.Data.Models.O2Ev;

namespace O2.Business.Data
{
    public class O2BusinessDataContext : DbContext
    {
        #region Ctors

        public O2BusinessDataContext(DbContextOptions<O2BusinessDataContext> options)
            : base(options)
        {

        }

        #endregion

        #region Fields

        #region O2Ev

        public virtual DbSet<O2EvEvent> O2EvEvent { get; set; }
        public virtual DbSet<O2EvMeta> O2EvMeta { get; set; }
        public virtual  DbSet<O2EvPhoto> O2EvPhoto { get; set; }
        #endregion
        
        #region O2C

        public virtual DbSet<O2CContact> O2CContact { get; set; }
        public virtual DbSet<O2CCertificate> O2CCertificate { get; set; }
        public virtual DbSet<O2CLocation> O2CLocation { get; set; }
        public virtual DbSet<O2CPhoto> O2CPhoto { get; set; }

        public virtual DbSet<O2CCertificateLocation> O2CCertificateLocation { get; set; }
        #endregion
        
        // public virtual DbSet<Audit> Audits { get; set; }
        
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<O2EvEvent>()
                .HasOne(a => a.Meta)
                .WithOne(b => b.O2EvEvent)
                .HasForeignKey<O2EvMeta>(b => b.EventId);
            
            
            modelBuilder.Entity<O2CCertificateLocation>().HasKey(bc => new { CertificateOwnerLocationId = bc.O2CLocationId, bc.O2CCertificateId });

            modelBuilder.Entity<O2CCertificateLocation>()
                .HasOne(bc => bc.O2CCertificate)
                .WithMany(b => b.Locations)
                .HasForeignKey(bc => bc.O2CCertificateId);

            modelBuilder.Entity<O2CCertificateLocation>()
                .HasOne(bc => bc.O2CLocation)
                .WithMany(c => c.O2CCertificateLocation)
                .HasForeignKey(bc => bc.O2CLocationId);

            modelBuilder.Entity<O2CPhoto>()
                .HasKey(e => new { e.Id, e.O2CCertificateId});

            modelBuilder.Entity<O2CPhoto>()
                .HasOne(e => e.O2CCertificate)
                .WithMany(e => e.Photos)
                .HasForeignKey(e => e.O2CCertificateId)
                .OnDelete(DeleteBehavior.Cascade); // <= This entity has cascading behaviour on deletion
            
            base.OnModelCreating(modelBuilder);
        }
        
        // public override int SaveChanges()
        // {
        //     UpdateAuditables();
        //     return base.SaveChanges();
        // }
        //
        // public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        // {
        //     UpdateAuditables();
        //     return base.SaveChangesAsync(cancellationToken);
        // }
        //
        // public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        // {
        //     UpdateAuditables();
        //     return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        // }

        // private void UpdateAuditables()
        // {
        //     var modifiedEntries = ChangeTracker.Entries<IAuditable>()
        //         .Where(c => c.State != EntityState.Unchanged && c.State != EntityState.Detached);
        //
        //     foreach (var modifiedEntry in modifiedEntries)
        //     {
        //         var modifiedProperties = modifiedEntry.Entity.GetType()
        //             .GetProperties()
        //             .Where(prop => !Attribute.IsDefined(prop, typeof(DoNotAudit)))
        //             .ToList();
        //
        //         foreach (var modifiedProperty in modifiedProperties)
        //         {
        //             var originalValue = modifiedEntry.OriginalValues[modifiedProperty.Name];
        //             var currentValue = modifiedEntry.CurrentValues[modifiedProperty.Name];
        //
        //             if (!Equals(originalValue, currentValue))
        //             {
        //                 AddAuditChange(modifiedEntry, modifiedProperty.Name, originalValue, currentValue);
        //             }
        //         }
        //     }
        // }
        //
        // private void AddAuditChange(EntityEntry<IAuditable> auditableEntity, string propertyName, object originalValue, object currentValue)
        // {
        //     var audit = new Audit
        //     {
        //         TableName = auditableEntity.Entity.GetType().Name,
        //         EntityId = ((IEntity) auditableEntity.Entity).Id,
        //         ChangeType = auditableEntity.State.ToString(),
        //         PropertyName = propertyName,
        //         OriginalValue = auditableEntity.State == EntityState.Added ? "[NEW]" : originalValue.ToString(),
        //         NewValue = currentValue.ToString(),
        //         IsAuditedOn = DateTime.Now,
        //         ModifiedBy = Thread.CurrentPrincipal == null  ? Thread.CurrentPrincipal?.Identity?.Name : "anynomous"
        //     };
        //
        //     Set<Audit>().Add(audit);
        // }
    }
    
    
}