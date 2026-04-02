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

        /// <summary>
        /// IMPLEMENT: Look up user by ID.
        /// return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        /// </summary>
        public async Task<User?> GetByIdAsync(Guid userId)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// IMPLEMENT: Look up user by email (used during login).
        /// Normalize the email before comparing.
        /// 
        /// var normalizedEmail = email.Trim().ToLowerInvariant();
        /// return await _dbContext.Users
        ///     .FirstOrDefaultAsync(u => u.Email == new Email(normalizedEmail));
        /// 
        /// Note: EF will use the value conversion to compare against the stored string.
        /// </summary>
        public async Task<User?> GetByEmailAsync(string email)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// IMPLEMENT: Check if email already exists (used during registration).
        /// 
        /// var normalizedEmail = email.Trim().ToLowerInvariant();
        /// return await _dbContext.Users
        ///     .AnyAsync(u => u.Email == new Email(normalizedEmail));
        /// </summary>
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// IMPLEMENT: Add a new user to the DbContext.
        /// await _dbContext.Users.AddAsync(user);
        /// </summary>
        public async Task AddAsync(User user)
        {
            // TODO: Implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// IMPLEMENT: Commit pending changes.
        /// await _dbContext.SaveChangesAsync();
        /// </summary>
        public async Task SaveChangesAsync()
        {
            // TODO: Implement
            throw new NotImplementedException();
        }
    }
}
