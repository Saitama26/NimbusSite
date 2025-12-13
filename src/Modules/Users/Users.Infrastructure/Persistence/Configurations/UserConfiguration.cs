using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasMaxLength(20);

        builder.Property(x => x.Bio)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.LastLoginAt);

        // Email глобально уникальный (не в рамках тенанта)
        builder.HasIndex(x => x.Email).IsUnique();
    }
}

