using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shortening.Domain;

namespace Shortening.Infrastructure.Persistence
{
    public class ShorteningDbContext : DbContext
    {
        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }
        private readonly IMediator _mediator;
        public ShorteningDbContext(DbContextOptions<ShorteningDbContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShorteningDbContext).Assembly);
            modelBuilder.HasDefaultSchema("shortening"); // Default schema so other modules can have their own schemas without conflict
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var entitiesWithEvents = ChangeTracker.Entries<Entity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .ToList();

            var domainEvents = ChangeTracker.Entries<Entity>()
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            var result = await base.SaveChangesAsync(ct);

            foreach(var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, ct);
            }

            entitiesWithEvents.ForEach(e => e.Entity.ClearDomainEvents());

            return result;
        }
    }
}
