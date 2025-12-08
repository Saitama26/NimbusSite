using Domain.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<Domain.Permissions.RolePermission>
{
    public void Configure(EntityTypeBuilder<Domain.Permissions.RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(rp => rp.RolePermissionId);

        builder.Property(rp => rp.RolePermissionId)
            .IsRequired();

        builder.Property(rp => rp.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(rp => rp.Permission)
            .IsRequired()
            .HasConversion<int>();

        // Индексы
        builder.HasIndex(rp => rp.Role);
        builder.HasIndex(rp => rp.Permission);
        builder.HasIndex(rp => new { rp.Role, rp.Permission })
            .IsUnique();
    }
}

