using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tasks.Infrastructure.Views.TenantsViews;

internal sealed class TenantViewConfiguration : IEntityTypeConfiguration<TenantView>
{
    public void Configure(EntityTypeBuilder<TenantView> builder)
    {
        builder.ToView("vw_Tenants");
        builder.HasNoKey();

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.Status).HasColumnName("Status");
        builder.Property(x => x.Subdomain).HasColumnName("Subdomain");
        builder.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt");
    }
}

