using Analytics.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Infrastructure.Persistence.Configurations
{
    public class ShortenedUrlStatsConfiguration : IEntityTypeConfiguration<ShortenedUrlStats>
    {
        public void Configure(EntityTypeBuilder<ShortenedUrlStats> builder)
        {
            builder.ToTable("ShortenedUrlStats");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ShortCode)
                .IsRequired()
                .HasMaxLength(8);

            builder.Property(x => x.OriginalUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.UserId)
                .IsRequired(false);

            builder.Property(x => x.TotalClicks)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.LastClickedAtUtc)
                .IsRequired(false);

            // One-to-many: Stats -> ClickEvents
            builder.HasMany(x => x.ClickEvents)
                .WithOne(x => x.ShortenedUrlStats)
                .HasForeignKey(x => x.ShortenedUrlStatsId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index on ShortCode for quick lookups when clicks happen
            builder.HasIndex(x => x.ShortCode)
                .IsUnique();

            // Index on UserId for user-specific analytics queries
            builder.HasIndex(x => x.UserId);

            // Ignore Domain Events
            builder.Ignore(x => x.DomainEvents);
        }
    }
}
