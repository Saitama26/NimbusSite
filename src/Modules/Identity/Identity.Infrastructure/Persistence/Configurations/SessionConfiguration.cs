using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Identity.Domain.Entities;
using Identity.Domain.Enums;

namespace Identity.Infrastructure.Persistence.Configurations;

/// <summary>
/// Конфигурация EF Core для Session
/// </summary>
internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(SessionStatus.Active);

        builder.Property(x => x.RefreshTokenId);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.LastActivityAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.ClosedAt);
        builder.Property(x => x.CloseReason)
            .HasMaxLength(500);

        // Индекс на UserId для поиска всех сессий пользователя
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_Sessions_UserId");

        // Индекс на RefreshTokenId для связи с токеном
        builder.HasIndex(x => x.RefreshTokenId)
            .HasDatabaseName("IX_Sessions_RefreshTokenId");

        // Индекс на (TenantId, UserId) для фильтрации
        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .HasDatabaseName("IX_Sessions_TenantId_UserId");

        // Индекс на Status для поиска активных сессий
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_Sessions_Status");

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}

