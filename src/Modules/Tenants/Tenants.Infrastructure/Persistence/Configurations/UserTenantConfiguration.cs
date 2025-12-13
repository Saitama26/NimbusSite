using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenants.Domain.Entities;

namespace Tenants.Infrastructure.Persistence.Configurations;

internal sealed class UserTenantConfiguration : IEntityTypeConfiguration<UserTenant>
{
    public void Configure(EntityTypeBuilder<UserTenant> builder)
    {
        builder.ToTable("UserTenants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.IsOwner)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Role)
            .IsRequired();

        builder.Property(x => x.JoinedAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Уникальный индекс: один пользователь может быть в одном тенанте только один раз
        builder.HasIndex(x => new { x.UserId, x.TenantId })
            .IsUnique();

        // Индекс для быстрого поиска тенантов пользователя
        builder.HasIndex(x => x.UserId);

        // Индекс для быстрого поиска пользователей тенанта
        builder.HasIndex(x => x.TenantId);
    }
}

