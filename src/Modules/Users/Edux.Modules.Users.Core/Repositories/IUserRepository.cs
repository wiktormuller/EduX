using Edux.Modules.Users.Core.Entities;
using Edux.Shared.Abstractions.SharedKernel.Types;

namespace Edux.Modules.Users.Core.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetAsync(AggregateId id);
        Task<User?> GetAsync(string email);
        Task AddAsync(User user);
        void Update(User user);
        Task SaveChangesAsync();
    }
}
