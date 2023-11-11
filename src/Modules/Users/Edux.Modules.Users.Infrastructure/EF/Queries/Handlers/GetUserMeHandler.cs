using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Modules.Users.Application.Queries;
using Edux.Modules.Users.Infrastructure.EF.Contexts;
using Edux.Modules.Users.Infrastructure.EF.ReadModels;
using Edux.Shared.Abstractions.Queries;
using Microsoft.EntityFrameworkCore;

namespace Edux.Modules.Users.Infrastructure.EF.Queries.Handlers
{
    internal sealed class GetUserMeHandler : IQueryHandler<GetUserMe, UserMeResponse?>
    {
        private readonly DbSet<UserReadModel> _users;

        public GetUserMeHandler(UsersReadDbContext dbContext)
        {
            _users = dbContext.Users;
        }

        public async Task<UserMeResponse?> HandleAsync(GetUserMe query)
        {
            return await _users
                .Where(u => u.Id == query.UserId)
                .Select(userReadModel => userReadModel.AsUserMeResponse())
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }
    }
}
