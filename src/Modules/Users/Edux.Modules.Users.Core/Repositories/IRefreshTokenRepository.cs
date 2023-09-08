using Edux.Modules.Users.Core.Entities;

namespace Edux.Modules.Users.Core.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetAsync(string token);
        Task AddAsync(RefreshToken token);
        Task UpdateAsync(RefreshToken token);
        Task SaveChangesAsync();
    }
}
