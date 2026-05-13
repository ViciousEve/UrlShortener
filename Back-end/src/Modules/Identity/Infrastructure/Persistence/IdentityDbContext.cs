using Microsoft.EntityFrameworkCore;
using Identity.Domain;

namespace Identity.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core DbContext for the Identity module.
    /// Same pattern as ShorteningDbContext in the Shortening module.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. Each module has its own DbContext — this is the modular monolith pattern.
    ///    It keeps modules isolated and allows them to evolve independently.
    ///    
    /// 2. The "identity" schema prevents table name collisions with other modules.
    ///    The Shortening module uses "shortening" schema.
    ///    
    /// 3. ApplyConfigurationsFromAssembly scans this assembly for all classes
    ///    implementing IEntityTypeConfiguration<T> (like UserConfiguration.cs)
    ///    and applies them automatically.
    ///    
    /// 4. When you run EF migrations, specify this context:
    ///    dotnet ef migrations add InitialIdentity --context IdentityDbContext
    ///    --project src/Modules/Identity --startup-project src/UrlShortener.API
    /// </summary>
    public class IdentityDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Scans for all IEntityTypeConfiguration<T> in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

            // Separate schema so Identity tables don't collide with other modules
            modelBuilder.HasDefaultSchema("identity");
        }

        // TODO: Add a domain event dispatcher (same TODO as in ShorteningDbContext)
        // Override SaveChangesAsync to dispatch domain events after saving
    }
}
