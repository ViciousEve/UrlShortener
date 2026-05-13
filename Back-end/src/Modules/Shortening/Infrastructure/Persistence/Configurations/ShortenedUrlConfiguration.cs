using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shortening.Domain;

namespace Shortening.Infrastructure.Persistence.Configurations
{
    public class ShortenedUrlConfiguration : IEntityTypeConfiguration<ShortenedUrl>
    {
        private const int MAX_CODE_LENGTH = 8;
        public void Configure(EntityTypeBuilder<ShortenedUrl> builder)
        {
            builder.ToTable("ShortenedUrls");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OriginalUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.ShortCode)
                .HasConversion(
                    v => v.Value, //write to db
                    v => new ShortCode(v)) //read from db
                .HasMaxLength(MAX_CODE_LENGTH)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.UserId)
                .IsRequired(false);

            //Index on ShortCode for faster lookups
            builder.HasIndex(x => x.ShortCode)
                .IsUnique();

            //Index for user-specific queries
            builder.HasIndex(x => x.UserId);

            //Index for expiration queries
            builder.HasIndex(x => new {x.Status, x.ExpiresAt});

            //Ignore Domain Events
            builder.Ignore(x => x.DomainEvents);
        }
    }
}
