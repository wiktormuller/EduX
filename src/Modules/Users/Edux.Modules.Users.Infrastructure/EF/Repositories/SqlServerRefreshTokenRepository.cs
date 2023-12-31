﻿using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Core.Repositories;
using Edux.Modules.Users.Infrastructure.EF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Edux.Modules.Users.Infrastructure.EF.Repositories
{
    internal class SqlServerRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly UsersWriteDbContext _dbContext;
        private readonly DbSet<RefreshToken> _refreshTokens;

        public SqlServerRefreshTokenRepository(UsersWriteDbContext dbContext)
        {
            _dbContext = dbContext;
            _refreshTokens = _dbContext.RefreshTokens;
        }

        public async Task AddAsync(RefreshToken token)
        {
            await _refreshTokens.AddAsync(token);
        }

        public Task<RefreshToken?> GetAsync(string token)
        {
            return _refreshTokens.SingleOrDefaultAsync(x =>  x.Token == token);
        }

        public void Update(RefreshToken token)
        {
            _refreshTokens.Update(token);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
