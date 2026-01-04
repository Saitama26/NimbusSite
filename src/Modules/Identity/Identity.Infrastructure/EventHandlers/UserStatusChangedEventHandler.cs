using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Identity.Domain.Enums;
using Users.Contracts.Enums;
using Users.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.EventHandlers;

/// <summary>
/// Обработчик события изменения статуса пользователя
/// Блокирует/разблокирует доступ в зависимости от статуса
/// </summary>
internal sealed class UserStatusChangedEventHandler : IEventHandler<UserStatusChangedEvent>
{
    private readonly EventHandlersTenantDbContextFactory _dbContextFactory;
    private readonly ILogger<UserStatusChangedEventHandler> _logger;

    public UserStatusChangedEventHandler(
        EventHandlersTenantDbContextFactory dbContextFactory,
        ILogger<UserStatusChangedEventHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task Handle(UserStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserStatusChangedEvent for UserId: {UserId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}, TenantId: {TenantId}",
            domainEvent.UserId,
            domainEvent.OldStatus,
            domainEvent.NewStatus,
            domainEvent.TenantId);

        // Создаем DbContext для тенанта из события
        using var dbContext = await _dbContextFactory.CreateDbContextForTenantAsync<IdentityDbContext>(
            domainEvent.TenantId,
            options => new IdentityDbContext(options),
            cancellationToken);

        var credentials = await dbContext.UserCredentials
            .FirstOrDefaultAsync(c => c.UserId == domainEvent.UserId, cancellationToken);
        if (credentials == null)
        {
            _logger.LogWarning(
                "UserCredentials not found for UserId: {UserId}. Skipping status change.",
                domainEvent.UserId);
            return;
        }

        // Если статус стал Inactive или Deleted - блокируем доступ
        if (domainEvent.NewStatus == UserStatusContract.Inactive || domainEvent.NewStatus == UserStatusContract.Deleted)
        {
            credentials.LockedOutUntil = DateTime.UtcNow.AddYears(100); // Фактическая блокировка
            credentials.UpdatedAt = DateTime.UtcNow;

            // Отзываем все активные сессии
            var sessions = await dbContext.Sessions
                .Where(s => s.UserId == domainEvent.UserId && s.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var session in sessions)
            {
                session.Status = SessionStatus.Revoked;
                session.ClosedAt = DateTime.UtcNow;
                session.CloseReason = $"User status changed to {domainEvent.NewStatus}";
            }

            // Отзываем все refresh токены
            var tokens = await dbContext.RefreshTokens
                .Where(t => t.UserId == domainEvent.UserId && t.IsValid)
                .ToListAsync(cancellationToken);

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevocationReason = $"User status changed to {domainEvent.NewStatus}";
            }
        }
        // Если статус стал Active - разблокируем доступ
        else if (domainEvent.NewStatus == UserStatusContract.Active && domainEvent.OldStatus != UserStatusContract.Active)
        {
            credentials.LockedOutUntil = null;
            credentials.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User credentials updated for UserId: {UserId}, NewStatus: {NewStatus}, TenantId: {TenantId}",
            domainEvent.UserId,
            domainEvent.NewStatus,
            domainEvent.TenantId);
    }
}

