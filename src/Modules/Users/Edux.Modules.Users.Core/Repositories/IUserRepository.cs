using Edux.Modules.Users.Core.Entities;
using Edux.Shared.Abstractions.Kernel.Types;

namespace Edux.Modules.Users.Core.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetAsync(AggregateId id);
        Task<User?> GetAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task SaveChangesAsync();
    }
}
