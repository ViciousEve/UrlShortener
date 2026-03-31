using Analytics.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Infrastructure.Persistence.Configurations
{
    public class ClickEventConfiguration : IEntityTypeConfiguration<ClickEvent>
    {
        public void Configure(EntityTypeBuilder<ClickEvent> builder)
        {
            builder.ToTable("ClickEvents");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ShortenedUrlStatsId)
                .IsRequired();

            builder.Property(x => x.ClickedAtUtc)
                .IsRequired();

            // Setup foreign key
            builder.HasOne(x => x.ShortenedUrlStats)
                .WithMany()
                .HasForeignKey(x => x.ShortenedUrlStatsId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting the stats deletes all its clicks

            // Index for querying clicks for a specific URL
            builder.HasIndex(x => x.ShortenedUrlStatsId);

            // Composite index for time-range queries per URL
            builder.HasIndex(x => new { x.ShortenedUrlStatsId, x.ClickedAtUtc });

            // Ignore Domain Events
            builder.Ignore(x => x.DomainEvents);
        }
    }
}
