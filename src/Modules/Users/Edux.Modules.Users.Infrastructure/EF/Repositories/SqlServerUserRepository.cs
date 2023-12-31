﻿using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Core.Repositories;
using Edux.Modules.Users.Infrastructure.EF.Contexts;
using Edux.Shared.Abstractions.SharedKernel.Types;
using Microsoft.EntityFrameworkCore;

namespace Edux.Modules.Users.Infrastructure.EF.Repositories
{
    internal sealed class SqlServerUserRepository : IUserRepository
    {
        private readonly UsersWriteDbContext _dbContext;
        private readonly DbSet<User> _users;

        public SqlServerUserRepository(UsersWriteDbContext dbContext)
        {
            _dbContext = dbContext;
            _users = _dbContext.Users;
        }

        public async Task AddAsync(User user)
        {
            await _users.AddAsync(user);
        }

        public Task<User?> GetAsync(AggregateId id)
        {
            return _users.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User?> GetAsync(string email)
        {
            return await _users.SingleOrDefaultAsync(x => x.Email == email);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public void Update(User user)
        {
            _users.Update(user);
        }
    }
}
