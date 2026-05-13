using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Domain;

namespace Identity.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id);

            // Email value object — stored as a plain string
            builder.Property(x => x.Email)
                .HasConversion(
                    v => v.Value,           // Write to DB: Email → string
                    v => new Email(v))      // Read from DB: string → Email
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.PasswordHash)
                .HasMaxLength(512)          // BCrypt hashes are ~60 chars
                .IsRequired();

            builder.Property(x => x.Username)
                .HasMaxLength(50)
                .IsRequired();

            // Role enum stored as string for readability in DB
            builder.Property(x => x.Role)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();

            // Unique index on email — prevents duplicates, fast login lookup
            builder.HasIndex(x => x.Email).IsUnique();

            // Ignore domain events — not persisted to DB
            builder.Ignore(x => x.DomainEvents);
        }
    }
}
