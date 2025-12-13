using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Domain.Entities;

namespace Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация EF Core для UserCredentials
/// </summary>
internal sealed class UserCredentialsConfiguration : IEntityTypeConfiguration<UserCredentials>
{
    public void Configure(EntityTypeBuilder<UserCredentials> builder)
    {
        builder.ToTable("UserCredentials");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.EmailConfirmed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.FailedLoginAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        // Уникальный индекс на (TenantId, Email) для быстрого поиска при логине
        builder.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique()
            .HasDatabaseName("IX_UserCredentials_TenantId_Email");

        // Индекс на UserId для связи с Users модулем
        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserCredentials_UserId");

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
        builder.Property(x => x.PasswordChangedAt);
        builder.Property(x => x.LockedOutUntil);
    }
}

