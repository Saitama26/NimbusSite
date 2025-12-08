using Domain.Projects;
using Domain.Projects.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .IsRequired();

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.StartDate)
            .IsRequired();

        builder.Property(p => p.EndDate);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(ProjectStatus.Active);

        builder.Property(p => p.OwnerId)
            .IsRequired();

        // Индексы
        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => new { p.TenantId, p.Name })
            .IsUnique();

        // Связи
        builder.HasOne(p => p.Owner)
            .WithMany()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь many-to-many с User (Members) настроена в UserConfiguration
    }
}

