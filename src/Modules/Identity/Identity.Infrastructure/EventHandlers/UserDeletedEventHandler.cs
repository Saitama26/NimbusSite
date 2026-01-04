using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Identity.Domain.Enums;
using Users.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.EventHandlers;

/// <summary>
/// Обработчик события удаления пользователя
/// Отзывает все активные сессии и токены
/// </summary>
internal sealed class UserDeletedEventHandler : IEventHandler<UserDeletedEvent>
{
    private readonly EventHandlersTenantDbContextFactory _dbContextFactory;
    private readonly ILogger<UserDeletedEventHandler> _logger;

    public UserDeletedEventHandler(
        EventHandlersTenantDbContextFactory dbContextFactory,
        ILogger<UserDeletedEventHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task Handle(UserDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserDeletedEvent for UserId: {UserId}, TenantId: {TenantId}",
            domainEvent.UserId,
            domainEvent.TenantId);

        // Создаем DbContext для тенанта из события
        using var dbContext = await _dbContextFactory.CreateDbContextForTenantAsync<IdentityDbContext>(
            domainEvent.TenantId,
            options => new IdentityDbContext(options),
            cancellationToken);

        // Отзываем все активные сессии
        var sessions = await dbContext.Sessions
            .Where(s => s.UserId == domainEvent.UserId && s.IsActive)
            .ToListAsync(cancellationToken);
        
        foreach (var session in sessions)
        {
            session.Status = SessionStatus.Revoked;
            session.ClosedAt = DateTime.UtcNow;
            session.CloseReason = "User deleted";
        }

        // Отзываем все refresh токены
        var tokens = await dbContext.RefreshTokens
            .Where(t => t.UserId == domainEvent.UserId && t.IsValid)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevocationReason = "User deleted";
        }

        // Блокируем учетные данные (не удаляем, чтобы сохранить историю)
        var credentials = await dbContext.UserCredentials
            .FirstOrDefaultAsync(c => c.UserId == domainEvent.UserId, cancellationToken);
        if (credentials != null)
        {
            credentials.LockedOutUntil = DateTime.UtcNow.AddYears(100); // Фактическая блокировка
            credentials.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User credentials locked and all sessions/tokens revoked for UserId: {UserId}, TenantId: {TenantId}",
            domainEvent.UserId,
            domainEvent.TenantId);
    }
}

