using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Tasks.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccessPermissions.Infrastructure.EventHandlers;

/// <summary>
/// Удаляет права, привязанные к удалённой задаче.
/// </summary>
internal sealed class TaskDeletedEventHandler : IEventHandler<TaskDeletedEvent>
{
    private readonly EventHandlersTenantDbContextFactory _dbContextFactory;
    private readonly ILogger<TaskDeletedEventHandler> _logger;

    public TaskDeletedEventHandler(
        EventHandlersTenantDbContextFactory dbContextFactory,
        ILogger<TaskDeletedEventHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task Handle(TaskDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling TaskDeletedEvent for TaskId: {TaskId}, TenantId: {TenantId}",
            domainEvent.TaskId,
            domainEvent.TenantId);

        // Создаем DbContext для тенанта из события
        using var dbContext = await _dbContextFactory.CreateDbContextForTenantAsync<AccessPermissionsDbContext>(
            domainEvent.TenantId,
            options => new AccessPermissionsDbContext(options),
            cancellationToken);

        var permissions = await dbContext.AccessPermissions
            .Where(p => p.TaskId == domainEvent.TaskId)
            .ToListAsync(cancellationToken);

        var count = permissions.Count;
        if (count > 0)
        {
            dbContext.AccessPermissions.RemoveRange(permissions);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Removed {Count} permissions for deleted TaskId {TaskId}, TenantId: {TenantId}",
            count,
            domainEvent.TaskId,
            domainEvent.TenantId);
    }
}

