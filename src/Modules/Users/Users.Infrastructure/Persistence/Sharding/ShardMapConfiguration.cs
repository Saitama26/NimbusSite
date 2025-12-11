using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Users.Infrastructure.Persistence.Sharding;

internal sealed class ShardMapConfiguration : IEntityTypeConfiguration<ShardMapEntry>
{
    public void Configure(EntityTypeBuilder<ShardMapEntry> builder)
    {
        builder.ToTable("ShardMap");

        builder.HasKey(x => x.TenantId);

        builder.Property(x => x.TenantId)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(x => x.ConnectionString)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ShardKey)
            .HasMaxLength(200);
    }
}

