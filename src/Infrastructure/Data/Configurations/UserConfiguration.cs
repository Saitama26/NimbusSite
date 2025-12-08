using Domain.Users;
using Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .IsRequired();

        builder.Property(u => u.TenantId)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(UserRole.Observer);

        // Индексы
        builder.HasIndex(u => u.TenantId);
        builder.HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique();

        // Связи
        builder.HasMany(u => u.Projects)
            .WithMany(p => p.Members)
            .UsingEntity<Dictionary<string, object>>(
                "ProjectMembers",
                j => j.HasOne<Domain.Projects.Project>()
                    .WithMany()
                    .HasForeignKey("ProjectId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<User>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("ProjectId", "UserId");
                    j.ToTable("ProjectMembers");
                });

        builder.HasMany(u => u.Tasks)
            .WithOne(t => t.AssignedUser)
            .HasForeignKey(t => t.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

