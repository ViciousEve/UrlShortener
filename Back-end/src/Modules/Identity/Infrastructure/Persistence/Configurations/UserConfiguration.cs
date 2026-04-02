using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Domain;

namespace Identity.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF Core entity configuration for the User entity.
    /// Same pattern as ShortenedUrlConfiguration in the Shortening module.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. This class is auto-discovered by ApplyConfigurationsFromAssembly
    ///    in IdentityDbContext.OnModelCreating.
    ///    
    /// 2. Fluent API is preferred over data annotations because:
    ///    - It keeps the domain entity clean (no EF attributes)
    ///    - All persistence config is in one place
    ///    - More powerful (can do things annotations can't)
    ///    
    /// 3. Value conversions allow EF to store value objects (Email) and enums (Role)
    ///    as simple database types (strings). The conversion maps:
    ///    - Email: User.Email → Email.Value (string) for write,
    ///             string → new Email(string) for read
    ///    - Role:  HasConversion<string>() stores the enum name as text
    ///    
    /// 4. Indexes are critical for performance:
    ///    - Email (unique): prevents duplicate accounts, fast login lookup
    ///    - Username: if you add search/display features later
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // TODO: Implement all the configuration below

            // builder.ToTable("Users");
            // builder.HasKey(x => x.Id);

            // -- Email value object conversion --
            // builder.Property(x => x.Email)
            //     .HasConversion(
            //         v => v.Value,           // Write to DB: Email → string
            //         v => new Email(v))       // Read from DB: string → Email
            //     .HasMaxLength(256)
            //     .IsRequired();

            // -- Password hash --
            // builder.Property(x => x.PasswordHash)
            //     .HasMaxLength(512)          // BCrypt hashes are ~60 chars, but leave room
            //     .IsRequired();

            // -- Username --
            // builder.Property(x => x.Username)
            //     .HasMaxLength(50)
            //     .IsRequired();

            // -- Role enum stored as string --
            // builder.Property(x => x.Role)
            //     .HasConversion<string>()    // Stores "User" or "Admin" as text
            //     .IsRequired();

            // builder.Property(x => x.CreatedAtUtc).IsRequired();
            // builder.Property(x => x.IsActive).IsRequired();

            // -- Indexes --
            // builder.HasIndex(x => x.Email).IsUnique();  // Prevent duplicate emails, fast login lookup

            // -- Ignore domain events (not persisted) --
            // builder.Ignore(x => x.DomainEvents);
        }
    }
}
