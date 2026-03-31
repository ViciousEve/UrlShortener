using Analytics.Application.Contracts;
using Analytics.Domain;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence
{
    public class ClickEventRepository : IClickEventRepository
    {
        private readonly AnalyticsDbContext _context;

        public ClickEventRepository(AnalyticsDbContext context)
        {
            _context = context;
        }

        public async Task AddClickAsync(ClickEvent clickEvent)
        {
            await _context.ClickEvents.AddAsync(clickEvent);
        }

        public async Task<IEnumerable<ClickEvent>> GetClicksByShortCodeAsync(string shortCode)
        {
            return await _context.ClickEvents
                .Include(c => c.ShortenedUrlStats)
                .Where(x => x.ShortenedUrlStats.ShortCode == shortCode)
                .OrderByDescending(x => x.ClickedAtUtc)
                .ToListAsync();
        }

        public async Task<ShortenedUrlStats?> GetStatsByIdAsync(Guid shortenedUrlStatsId)
        {
            return await _context.ShortenedUrlStats
                .FirstOrDefaultAsync(s => s.Id == shortenedUrlStatsId);
        }

        public async Task AddStatsAsync(ShortenedUrlStats stats)
        {
            await _context.ShortenedUrlStats.AddAsync(stats);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
