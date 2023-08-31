using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Infrastructure.EF.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Edux.Modules.Users.Infrastructure.EF.Contexts
{
    internal class RefreshTokensDbContext : DbContext
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public RefreshTokensDbContext(DbContextOptions<RefreshTokensDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("users");

            modelBuilder.ApplyConfiguration(new RefreshTokensConfiguration());
            modelBuilder.ApplyConfiguration(new UsersWriteConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
