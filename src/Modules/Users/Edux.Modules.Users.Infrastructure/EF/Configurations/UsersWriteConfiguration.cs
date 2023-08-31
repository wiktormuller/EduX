﻿using Edux.Modules.Users.Core.Entities;
using Edux.Modules.Users.Core.ValueObjects;
using Edux.Shared.Abstractions.Kernel.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Edux.Modules.Users.Infrastructure.EF.Configurations
{
    internal sealed class UsersWriteConfiguration : IEntityTypeConfiguration<User>
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasConversion(x => x.Value, x => new AggregateId(x));

            builder.Property(u => u.Email)
                .HasConversion(x => x.Value, x => new Email(x))
                .IsRequired();

            builder.HasIndex(x => x.Username)
                .IsUnique();

            builder.Property(u => u.Username)
                .HasConversion(x => x.Value, x => new Username(x))
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(u => u.Role)
                .HasConversion(x => x.Value, x => new Role(x))
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(u => u.Password)
                .HasConversion(x => x.Value, x => new Password(x))
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Claims)
            .HasConversion(x => JsonSerializer.Serialize(x, SerializerOptions),
                x => JsonSerializer.Deserialize<Dictionary<string, IEnumerable<string>>>(x, SerializerOptions));

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.UpdatedAt)
                .IsRequired();

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.HasIndex(u => u.Id);

            builder.HasMany<RefreshToken>()
                .WithOne()
                .HasForeignKey(e => e.UserId)
                .IsRequired();
        }
    }
}