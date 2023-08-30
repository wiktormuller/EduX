using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Infrastructure.EF.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Edux.Modules.Users.Infrastructure.EF.Contexts
{
    internal class UsersWriteDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public UsersWriteDbContext(DbContextOptions<UsersWriteDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("users");

            modelBuilder.ApplyConfiguration(new UsersWriteConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
