using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Users.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccessPermissions.Infrastructure.EventHandlers;

/// <summary>
/// Удаляет права доступа для удаленного пользователя.
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
        using var dbContext = await _dbContextFactory.CreateDbContextForTenantAsync<AccessPermissionsDbContext>(
            domainEvent.TenantId,
            options => new AccessPermissionsDbContext(options),
            cancellationToken);

        var permissions = await dbContext.AccessPermissions
            .Where(p => p.UserId == domainEvent.UserId)
            .ToListAsync(cancellationToken);

        var count = permissions.Count;
        if (count > 0)
        {
            dbContext.AccessPermissions.RemoveRange(permissions);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Removed {Count} permissions for deleted UserId {UserId}, TenantId: {TenantId}",
            count,
            domainEvent.UserId,
            domainEvent.TenantId);
    }
}

