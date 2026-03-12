using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shortening.Application.Contracts;
using Shortening.Domain;

namespace Shortening.Infrastructure.Persistence
{
    public class ShortenedUrlRepository : IShortenedUrlRepository
    {
        private readonly ShorteningDbContext _context;
        public ShortenedUrlRepository(ShorteningDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(ShortenedUrl shortenedUrl)
        {
            //add to database
            _context.Add(shortenedUrl);
        }

        public async Task DeleteAsync(string shortCode)
        {
            var shortenedUrl = await _context.ShortenedUrls.FirstOrDefaultAsync(x => x.ShortCode == new ShortCode(shortCode));
            if(shortenedUrl != null)
            {
                _context.Remove(shortenedUrl);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode)
        {
            return await _context.ShortenedUrls.FirstOrDefaultAsync(x => x.ShortCode == new ShortCode(shortCode));          
        }

        public async Task<IEnumerable<ShortenedUrl>> GetByUserIdAsync(Guid userId)
        {
            return await _context.ShortenedUrls.Where(x => x.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<ShortenedUrl>> GetExpiredUrlsAsync()
        {
            return await _context.ShortenedUrls.Where(x => x.Status == UrlStatus.Active && x.ExpiresAt < DateTime.UtcNow).ToListAsync();
        }
    }
}
