using Microsoft.Extensions.Caching.Memory;
using Shortening.Application.Contracts;
using Shortening.Domain;

namespace Shortening.Infrastructure.Persistence;

/// <summary>
/// Decorator that wraps <see cref="ShortenedUrlRepository"/> with in-memory caching
/// for fast redirect lookups on the hot path (GetByShortCodeAsync).
/// </summary>
public class CachedShortenedUrlRepository : IShortenedUrlRepository
{
    private readonly ShortenedUrlRepository _inner;
    private readonly IMemoryCache _cache;

    private const string CacheKeyPrefix = "shorturl:";
    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    public CachedShortenedUrlRepository(ShortenedUrlRepository inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode)
    {
        var cacheKey = $"{CacheKeyPrefix}{shortCode}";

        if (_cache.TryGetValue(cacheKey, out ShortenedUrl? cached))
        {
            return cached;
        }

        var result = await _inner.GetByShortCodeAsync(shortCode);

        if (result is not null)
        {
            _cache.Set(cacheKey, result, CacheOptions);
        }

        return result;
    }

    public async Task<IEnumerable<ShortenedUrl>> GetByUserIdAsync(Guid userId)
    {
        // User URL lists are not cached — low frequency, high variability
        return await _inner.GetByUserIdAsync(userId);
    }

    public async Task AddAsync(ShortenedUrl shortenedUrl)
    {
        await _inner.AddAsync(shortenedUrl);
    }

    public async Task DeleteAsync(string shortCode)
    {
        // Invalidate cache on delete
        _cache.Remove($"{CacheKeyPrefix}{shortCode}");
        await _inner.DeleteAsync(shortCode);
    }

    public async Task SaveChangesAsync()
    {
        await _inner.SaveChangesAsync();
    }

    public async Task<IEnumerable<ShortenedUrl>> GetExpiredUrlsAsync()
    {
        return await _inner.GetExpiredUrlsAsync();
    }
}
