using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenants.Domain.Entities;

namespace Tenants.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Таблица Tenants в центральной БД NimbusSite_Tenants
        builder.ToTable("Tenants");

        builder.HasKey(x => x.TenantInt);

        // TenantInt - автоинкремент
        builder.Property(x => x.TenantInt)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Индекс для быстрого поиска по имени
        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.ConnectionString)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>(); // Сохраняем как int в БД

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
    }
}

