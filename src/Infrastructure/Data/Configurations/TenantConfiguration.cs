using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .IsRequired();

        builder.Property(t => t.TenantId)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.ConnectionString)
            .IsRequired()
            .HasMaxLength(500);

        // Индексы
        builder.HasIndex(t => t.TenantId)
            .IsUnique();
        builder.HasIndex(t => t.Name)
            .IsUnique();

        // Примечание: Связи с User и Project через TenantId настроены через индексы
        // в соответствующих конфигурациях, так как навигационные свойства отсутствуют
    }
}

