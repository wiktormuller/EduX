using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Modules.Users.Application.Queries;
using Edux.Modules.Users.Infrastructure.EF.Contexts;
using Edux.Modules.Users.Infrastructure.EF.ReadModels;
using Edux.Shared.Abstractions.Queries;
using Microsoft.EntityFrameworkCore;

namespace Edux.Modules.Users.Infrastructure.EF.Queries.Handlers
{
    internal sealed class GetUsersHandler : IQueryHandler<GetUsers, IEnumerable<UserResponse>>
    {
        private readonly DbSet<UserReadModel> _users;

        public GetUsersHandler(UsersReadDbContext dbContext)
        {
            _users = dbContext.Users;
        }

        public async Task<IEnumerable<UserResponse>> HandleAsync(GetUsers query)
        {
            return await _users
                .Select(u => u.AsUserResponse())
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
