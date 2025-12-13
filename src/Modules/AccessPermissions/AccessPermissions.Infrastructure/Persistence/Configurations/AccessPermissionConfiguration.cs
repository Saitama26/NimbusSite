using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AccessPermissions.Domain.Entities;

namespace AccessPermissions.Infrastructure.Persistence.Configurations;

internal sealed class AccessPermissionConfiguration : IEntityTypeConfiguration<AccessPermission>
{
    public void Configure(EntityTypeBuilder<AccessPermission> builder)
    {
        builder.ToTable("AccessPermissions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ProjectId);

        builder.Property(x => x.TaskId);

        builder.Property(x => x.Scope)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Action)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CreatedByUserId)
            .IsRequired();

        builder.Property(x => x.ExpiresAt);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Индексы для оптимизации запросов
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.TaskId);
        builder.HasIndex(x => new { x.TenantId, x.UserId });
        builder.HasIndex(x => new { x.UserId, x.ProjectId });
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.Scope, x.Action, x.Type });

        // Уникальный индекс для предотвращения дубликатов
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.Scope, x.Action, x.Type, x.ProjectId, x.TaskId })
            .IsUnique();
    }
}

