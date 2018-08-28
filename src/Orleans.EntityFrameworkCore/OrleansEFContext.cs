using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    public class OrleansEFContext : DbContext
    {
        /// <summary>
        /// Gets or sets the memberships.
        /// </summary>
        /// <value>
        /// The memberships.
        /// </value>
        public DbSet<OrleansEFMembership> Memberships { get; set; }

        /// <summary>
        /// Gets or sets the membership versions.
        /// </summary>
        /// <value>
        /// The membership versions.
        /// </value>
        public DbSet<OrleansEFMembershipVersion> MembershipVersions { get; set; }

        /// <summary>
        /// Gets or sets the reminders.
        /// </summary>
        /// <value>
        /// The reminders.
        /// </value>
        public DbSet<OrleansEFReminder> Reminders { get; set; }

        /// <summary>
        /// Gets or sets the storage.
        /// </summary>
        /// <value>
        /// The storage.
        /// </value>
        public DbSet<OrleansEFStorage> Storage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrleansEFContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public OrleansEFContext(DbContextOptions<OrleansEFContext> options) : base(options)
        {
        }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        /// then this method will not be run.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrleansEFMembership>()
                .HasKey(a => new { a.DeploymentId, a.Address, a.Port, a.Generation });

            modelBuilder.Entity<OrleansEFMembershipVersion>()
                .HasKey(a => a.DeploymentId);

            modelBuilder.Entity<OrleansEFReminder>()
                .HasKey(a => new { a.ServiceId, a.GrainId, a.ReminderName });
        }

        /// <summary>
        /// Automatics the migrate.
        /// </summary>
        /// <returns></returns>
        public async Task AutoMigrate()
        {
            await Database.MigrateAsync();
        }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>
        /// The number of state entries written to the database.
        /// </returns>
        /// <remarks>
        /// This method will automatically call <see cref="M:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges" /> to discover any
        /// changes to entity instances before saving to the underlying database. This can be disabled via
        /// <see cref="P:Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.AutoDetectChangesEnabled" />.
        /// </remarks>
        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Saves the changes asynchronous.
        /// </summary>
        /// <returns></returns>
        public Task<int> SaveChangesAsync()
        {
            AddTimestamps();
            return base.SaveChangesAsync();
        }

        /// <summary>
        /// Adds the timestamps.
        /// </summary>
        private void AddTimestamps()
        {
            var entities = ChangeTracker
                .Entries()
                .Where(x =>
                    x.Entity is OrleansEFEntity &&
                    (x.State == EntityState.Added ||
                        x.State == EntityState.Modified)
                );

            foreach (var entity in entities)
            {
                var now = DateTime.UtcNow;

                if (entity.State == EntityState.Added)
                    ((OrleansEFEntity)entity.Entity).CreatedAt = now;

                ((OrleansEFEntity)entity.Entity).UpdatedAt = now;
            }
        }
    }
}