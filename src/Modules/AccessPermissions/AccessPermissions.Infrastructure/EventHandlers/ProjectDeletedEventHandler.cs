using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Projects.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccessPermissions.Infrastructure.EventHandlers;

/// <summary>
/// Удаляет права, привязанные к удалённому проекту.
/// </summary>
internal sealed class ProjectDeletedEventHandler : IEventHandler<ProjectDeletedEvent>
{
    private readonly EventHandlersTenantDbContextFactory _dbContextFactory;
    private readonly ILogger<ProjectDeletedEventHandler> _logger;

    public ProjectDeletedEventHandler(
        EventHandlersTenantDbContextFactory dbContextFactory,
        ILogger<ProjectDeletedEventHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task Handle(ProjectDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling ProjectDeletedEvent for ProjectId: {ProjectId}, TenantId: {TenantId}",
            domainEvent.ProjectId,
            domainEvent.TenantId);

        // Создаем DbContext для тенанта из события
        using var dbContext = await _dbContextFactory.CreateDbContextForTenantAsync<AccessPermissionsDbContext>(
            domainEvent.TenantId,
            options => new AccessPermissionsDbContext(options),
            cancellationToken);

        var permissions = await dbContext.AccessPermissions
            .Where(p => p.ProjectId == domainEvent.ProjectId)
            .ToListAsync(cancellationToken);

        var count = permissions.Count;
        if (count > 0)
        {
            dbContext.AccessPermissions.RemoveRange(permissions);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Removed {Count} permissions for deleted ProjectId {ProjectId}, TenantId: {TenantId}",
            count,
            domainEvent.ProjectId,
            domainEvent.TenantId);
    }
}

