using Analytics.Domain;

namespace Analytics.Application.Contracts
{
    public interface IClickEventRepository
    {
        Task AddClickAsync(ClickEvent clickEvent);
        Task<IEnumerable<ClickEvent>> GetClicksByShortCodeAsync(string shortCode);
        
        Task<ShortenedUrlStats?> GetStatsByIdAsync(Guid shortenedUrlStatsId);
        Task AddStatsAsync(ShortenedUrlStats stats);
        
        Task SaveChangesAsync();
    }
}
