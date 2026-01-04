using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Projects.Domain.Entities;

namespace Projects.Infrastructure.Persistence.Configurations;

internal sealed class ProjectUserConfiguration : IEntityTypeConfiguration<ProjectUser>
{
    public void Configure(EntityTypeBuilder<ProjectUser> builder)
    {
        builder.ToTable("ProjectUsers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Role)
            .IsRequired()
            .HasConversion<int>(); // Сохраняем enum как int

        builder.Property(x => x.JoinedAt)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Уникальный индекс на ProjectId + UserId (пользователь может быть только один раз в проекте)
        builder.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();

        // Индексы для быстрого поиска
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.UserId);
    }
}

