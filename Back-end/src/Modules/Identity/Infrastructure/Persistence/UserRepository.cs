using Microsoft.EntityFrameworkCore;
using Identity.Application.Contracts;
using Identity.Domain;

namespace Identity.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core implementation of IUserRepository.
    /// Same pattern as ShortenedUrlRepository in the Shortening module.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. Uses IdentityDbContext (injected via DI) for all data access.
    ///    
    /// 2. All methods are async and use EF Core's async LINQ extensions:
    ///    - FirstOrDefaultAsync for single-record lookups
    ///    - AnyAsync for existence checks
    ///    - AddAsync for inserts
    ///    
    /// 3. SaveChangesAsync commits the Unit of Work — call it from the handler
    ///    after all changes are made (not inside individual repository methods).
    ///    
    /// 4. For GetByEmailAsync, remember that Email is stored as a string in the DB
    ///    (via the value conversion), so you query against the raw string value.
    ///    Normalize the input email (trim + lowercase) before comparing.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDbContext _dbContext;

        public UserRepository(IdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            return await _dbContext.Users.
                AsNoTracking().
                FirstOrDefaultAsync(u => u.Email == new Email(normalizedEmail));
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant(); 
            return await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == new Email(normalizedEmail));
        }

        public async Task AddAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
        }

 
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
