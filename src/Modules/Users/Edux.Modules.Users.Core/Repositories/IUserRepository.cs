using Edux.Modules.Users.Core.Entities;

namespace Edux.Modules.Users.Core.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetAsync(Guid id);
        Task<User?> GetAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
