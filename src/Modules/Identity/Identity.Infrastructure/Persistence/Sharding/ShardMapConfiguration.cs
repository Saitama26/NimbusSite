using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Infrastructure.Persistence.Sharding;

namespace Identity.Infrastructure.Persistence.Sharding;

/// <summary>
/// Конфигурация EF Core для ShardMapEntry
/// </summary>
internal sealed class ShardMapConfiguration : IEntityTypeConfiguration<ShardMapEntry>
{
    public void Configure(EntityTypeBuilder<ShardMapEntry> builder)
    {
        builder.ToTable("ShardMapEntries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.ConnectionString)
            .IsRequired()
            .HasMaxLength(1000);

        // Уникальный индекс на TenantId
        builder.HasIndex(x => x.TenantId)
            .IsUnique()
            .HasDatabaseName("IX_ShardMapEntries_TenantId");

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}

