using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenants.Domain.Entities;

namespace Tenants.Infrastructure.Persistence.Configurations;

internal sealed class UserTenantConfiguration : IEntityTypeConfiguration<UserTenant>
{
    public void Configure(EntityTypeBuilder<UserTenant> builder)
    {
        // Таблица UserTenants в центральной БД NimbusSite_Tenants
        builder.ToTable("UserTenants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TenantInt)
            .IsRequired();

        builder.Property(x => x.IsOwner)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Role)
            .IsRequired()
            .HasConversion<int>(); // Сохраняем как int в БД

        builder.Property(x => x.JoinedAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Уникальный индекс: один пользователь может быть в одном тенанте только один раз
        builder.HasIndex(x => new { x.UserId, x.TenantInt })
            .IsUnique();

        // Индекс для быстрого поиска тенантов пользователя
        builder.HasIndex(x => x.UserId);

        // Индекс для быстрого поиска пользователей тенанта
        builder.HasIndex(x => x.TenantInt);

        // Foreign key к Tenant
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantInt)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

