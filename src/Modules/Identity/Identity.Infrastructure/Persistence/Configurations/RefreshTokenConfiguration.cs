using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Domain.Entities;

namespace Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация EF Core для RefreshToken
/// </summary>
internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.RevokedAt);
        builder.Property(x => x.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.RevocationReason)
            .HasMaxLength(500);

        // Индекс на UserId для поиска всех токенов пользователя
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        // Индекс на TokenHash для быстрого поиска при обновлении токена
        builder.HasIndex(x => x.TokenHash)
            .HasDatabaseName("IX_RefreshTokens_TokenHash");

        // Индекс на (TenantId, UserId) для фильтрации
        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .HasDatabaseName("IX_RefreshTokens_TenantId_UserId");

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}

