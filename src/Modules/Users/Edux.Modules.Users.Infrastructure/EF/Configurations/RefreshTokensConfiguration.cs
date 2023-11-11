using Edux.Modules.Users.Core.Entities;
using Edux.Shared.Abstractions.SharedKernel.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Edux.Modules.Users.Infrastructure.EF.Configurations
{
    internal class RefreshTokensConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                .HasConversion(x => x!.Value, x => new AggregateId(x));

            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(rt => rt.CreatedAt)
                .IsRequired();

            builder.Property(rt => rt.UserId)
                .HasConversion(x => x.Value, x => new AggregateId(x))
                .IsRequired();

            builder.HasIndex(x => x.Token);

            builder.Ignore(x => x.Revoked);
        }
    }
}
