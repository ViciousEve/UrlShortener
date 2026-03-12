using Shortening.Domain;

namespace Shortening.Application.Contracts
{
    public interface IShortenedUrlRepository
    {
        Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode);
        Task<IEnumerable<ShortenedUrl>> GetByUserIdAsync(Guid userId);
        Task SaveChangesAsync();
        Task AddAsync(ShortenedUrl shortenedUrl);
        Task DeleteAsync(string shortCode);
        Task<IEnumerable<ShortenedUrl>> GetExpiredUrlsAsync();
    }
}