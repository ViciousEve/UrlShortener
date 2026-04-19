using Analytics.Domain;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence
{
    public class AnalyticsDbContext : DbContext
    {
        public DbSet<ShortenedUrlStats> ShortenedUrlStats { get; set; }
        public DbSet<ClickEvent> ClickEvents { get; set; }

        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);
            modelBuilder.HasDefaultSchema("analytics");
        }
    }
}
