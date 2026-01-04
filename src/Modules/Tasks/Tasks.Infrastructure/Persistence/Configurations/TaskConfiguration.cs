using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasks.Domain.Entities;
using DomainTask = Tasks.Domain.Entities.Task;

namespace Tasks.Infrastructure.Persistence.Configurations;

internal sealed class  TaskConfiguration : IEntityTypeConfiguration<DomainTask>
{
    public void Configure(EntityTypeBuilder<DomainTask> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.ProjectId)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.CreatedByUserId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.DueDate);
        builder.Property(x => x.StartedAt);
        builder.Property(x => x.CompletedAt);

        // Индексы для оптимизации запросов
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.AssignedToUserId);
        builder.HasIndex(x => new { x.TenantId, x.ProjectId, x.Title });
    }
}

