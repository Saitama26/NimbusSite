using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Users.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.EventHandlers;

/// <summary>
/// Обработчик события обновления пользователя
/// Синхронизирует email в учетных данных, если он изменился
/// </summary>
internal sealed class UserUpdatedEventHandler : IEventHandler<UserUpdatedEvent>
{
    private readonly EventHandlersTenantDbContextFactory _dbContextFactory;
    private readonly ILogger<UserUpdatedEventHandler> _logger;

    public UserUpdatedEventHandler(
        EventHandlersTenantDbContextFactory dbContextFactory,
        ILogger<UserUpdatedEventHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task Handle(UserUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserUpdatedEvent for UserId: {UserId}, TenantId: {TenantId}",
            domainEvent.UserId,
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
                "UserCredentials not found for UserId: {UserId}. Skipping update.",
                domainEvent.UserId);
            return;
        }

        // Обновляем UpdatedAt
        credentials.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UserCredentials updated for UserId: {UserId}, TenantId: {TenantId}",
            domainEvent.UserId,
            domainEvent.TenantId);
    }
}

