using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shortening.Domain;

namespace Shortening.Infrastructure.Persistence
{
    public class ShorteningDbContext : DbContext
    {
        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
        public ShorteningDbContext(DbContextOptions<ShorteningDbContext> options) : base(options)
        {          
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShorteningDbContext).Assembly);
            modelBuilder.HasDefaultSchema("shortening"); // Default schema so other modules can have their own schemas without conflict
        }
        //TODO: add a domain event dispatcher        
         
    }
}
