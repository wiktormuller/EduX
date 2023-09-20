using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Modules.Users.Application.Queries;
using Edux.Modules.Users.Infrastructure.EF.Contexts;
using Edux.Modules.Users.Infrastructure.EF.ReadModels;
using Edux.Shared.Abstractions.Queries;
using Microsoft.EntityFrameworkCore;

namespace Edux.Modules.Users.Infrastructure.EF.Queries.Handlers
{
    internal sealed class GetUserDetailsHandler : IQueryHandler<GetUserDetails, UserDetailsResponse>
    {
        private readonly DbSet<UserReadModel> _users;

        public GetUserDetailsHandler(UsersReadDbContext dbContext)
        {
            _users = dbContext.Users;
        }

        public async Task<UserDetailsResponse> HandleAsync(GetUserDetails query)
        {
            return await _users
                .Where(u => u.Id == query.UserId)
                .Select(userReadModel => userReadModel.AsUserDetailsResponse())
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }
    }
}
