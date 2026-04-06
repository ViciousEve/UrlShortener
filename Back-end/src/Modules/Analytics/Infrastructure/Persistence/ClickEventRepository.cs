using Analytics.Application.Contracts;
using Analytics.Domain;
using Microsoft.EntityFrameworkCore;
using Analytics.Application.DTOs;

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
                .AsNoTracking()
                .Include(c => c.ShortenedUrlStats)
                .Where(x => x.ShortenedUrlStats.ShortCode == shortCode)
                .OrderByDescending(x => x.ClickedAtUtc)
                .ToListAsync();
        }

        public async Task<ShortenedUrlStats?> GetStatsByIdAsync(Guid shortenedUrlStatsId)
        {
            return await _context.ShortenedUrlStats
                .AsNoTracking()
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

        public async Task<int> GetTotalClickForUserInPeriodAsync(Guid userId, DateTime fromDate, DateTime toDate)
        {
            return await _context.ClickEvents
                .Where(x => x.ShortenedUrlStats.UserId == userId && x.ClickedAtUtc >= fromDate && x.ClickedAtUtc <= toDate)
                .CountAsync();
        }

        public async Task<IEnumerable<ShortenedUrlClickStats>> GetTotalClickForUserRankedAsync(Guid userId)
        {
            return await _context.ShortenedUrlStats
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.TotalClicks)
                .Select(x => new ShortenedUrlClickStats(x.Id, x.ShortCode, x.TotalClicks))
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetTopBrowserForUserAsync(Guid userId, int topN)
        {
            return await _context.ClickEvents
                .AsNoTracking()
                .Where(x => x.ShortenedUrlStats.UserId == userId)
                .GroupBy(x => x.UserAgent)
                .OrderByDescending(x => x.Count())
                .Take(topN)
                .Select(x => x.Key)
                .ToListAsync();
        }
    }
}
