using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Views.UsersViews;

internal sealed class UserViewConfiguration : IEntityTypeConfiguration<UserView>
{
    public void Configure(EntityTypeBuilder<UserView> builder)
    {
        builder.ToView("vw_Users");
        builder.HasNoKey();

        builder.Property(x => x.Id).HasColumnName("Id");
        builder.Property(x => x.TenantId).HasColumnName("TenantId");
        builder.Property(x => x.Email).HasColumnName("Email");
        builder.Property(x => x.Name).HasColumnName("Name");
        builder.Property(x => x.Status).HasColumnName("Status");
        builder.Property(x => x.UpdatedAt).HasColumnName("UpdatedAt");
    }
}

