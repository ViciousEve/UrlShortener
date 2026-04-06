using Analytics.Domain;
using Analytics.Application.DTOs;

namespace Analytics.Application.Contracts
{
    public interface IClickEventRepository
    {
        Task AddClickAsync(ClickEvent clickEvent);
        Task<IEnumerable<ClickEvent>> GetClicksByShortCodeAsync(string shortCode);
        Task<int> GetTotalClickForUserInPeriodAsync(Guid userId, DateTime fromDate, DateTime toDate);
        Task<ShortenedUrlStats?> GetStatsByIdAsync(Guid shortenedUrlStatsId);
        Task<IEnumerable<ShortenedUrlClickStats>> GetTotalClickForUserRankedAsync(Guid userId);
        Task<IEnumerable<string>> GetTopBrowserForUserAsync(Guid userId, int topN);
        Task AddStatsAsync(ShortenedUrlStats stats);
        
        Task SaveChangesAsync();

    }
}
