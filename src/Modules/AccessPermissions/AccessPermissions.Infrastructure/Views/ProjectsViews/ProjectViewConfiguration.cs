using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessPermissions.Infrastructure.Views.ProjectsViews;

internal sealed class ProjectViewConfiguration : IEntityTypeConfiguration<ProjectView>
{
    public void Configure(EntityTypeBuilder<ProjectView> builder)
    {
        builder.ToView("vw_Projects");
        builder.HasNoKey();

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.TenantId).HasColumnName("TenantId");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.Status).HasColumnName("Status");
        builder.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt");
    }
}

