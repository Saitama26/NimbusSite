using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Tenants.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccessPermissions.Infrastructure.EventHandlers;

/// <summary>
/// Удаляет права для удалённого тенанта.
/// </summary>
internal sealed class TenantDeletedEventHandler : IEventHandler<TenantDeletedEvent>
{
    private readonly EventHandlersTenantDbContextFactory _dbContextFactory;
    private readonly ILogger<TenantDeletedEventHandler> _logger;

    public TenantDeletedEventHandler(
        EventHandlersTenantDbContextFactory dbContextFactory,
        ILogger<TenantDeletedEventHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task Handle(TenantDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling TenantDeletedEvent for TenantInt: {TenantInt}",
            domainEvent.TenantInt);

        // Создаем DbContext для тенанта из события
        using var dbContext = await _dbContextFactory.CreateDbContextForTenantAsync<AccessPermissionsDbContext>(
            domainEvent.TenantInt,
            options => new AccessPermissionsDbContext(options),
            cancellationToken);

        var permissions = await dbContext.AccessPermissions
            .Where(p => p.TenantId == domainEvent.TenantInt)
            .ToListAsync(cancellationToken);

        var count = permissions.Count;
        if (count > 0)
        {
            dbContext.AccessPermissions.RemoveRange(permissions);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Removed {Count} permissions for deleted TenantId {TenantId}",
            count,
            domainEvent.TenantInt);
    }
}

