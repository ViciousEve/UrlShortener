using Identity.Domain;

namespace Identity.Application.Contracts
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid userId);

        Task<User?> GetByEmailAsync(string email);

        Task<bool> ExistsByEmailAsync(string email);

        Task AddAsync(User user);

        Task SaveChangesAsync();
    }
}
