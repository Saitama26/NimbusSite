using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessPermissions.Infrastructure.Views.TasksViews;

internal sealed class TaskViewConfiguration : IEntityTypeConfiguration<TaskView>
{
    public void Configure(EntityTypeBuilder<TaskView> builder)
    {
        builder.ToView("vw_Tasks");
        builder.HasNoKey();

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.ProjectId).HasColumnName("ProjectId");
        builder.Property(x => x.TenantId).HasColumnName("TenantId");
        builder.Property(x => x.Title).HasColumnName("Title");
        builder.Property(x => x.Status).HasColumnName("Status");
        builder.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt");
    }
}

