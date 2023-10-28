using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Infrastructure.EF.Configurations;
using Microsoft.EntityFrameworkCore;
using Edux.Shared.Abstractions.Messaging.Outbox;
using Edux.Shared.Abstractions.Messaging.Inbox;

namespace Edux.Modules.Users.Infrastructure.EF.Contexts
{
    internal class UsersWriteDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<OutboxMessage> Outbox { get; set; }
        public DbSet<InboxMessage> Inbox { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public UsersWriteDbContext(DbContextOptions<UsersWriteDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("users");

            modelBuilder.ApplyConfiguration(new UsersWriteConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokensConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
