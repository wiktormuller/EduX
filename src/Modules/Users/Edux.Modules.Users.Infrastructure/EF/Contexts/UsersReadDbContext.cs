﻿using Edux.Modules.Users.Infrastructure.EF.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Edux.Modules.Users.Infrastructure.EF.Contexts
{
    internal class UsersReadDbContext : DbContext
    {
        public DbSet<UserReadModel> Users { get; set; }

        public UsersReadDbContext(DbContextOptions<UsersReadDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("users");
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
