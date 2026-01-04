using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Users.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Projects.Infrastructure;

namespace Projects.Infrastructure.EventHandlers;

/// <summary>
/// Удаляет пользователя из всех проектов при его удалении
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

    public async Task Handle(UserDeletedEvent integrationEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserDeletedEvent for UserId: {UserId}, TenantId: {TenantId}",
            integrationEvent.UserId,
            integrationEvent.TenantId);

        // Создаем DbContext для тенанта из события
        using var dbContext = await _dbContextFactory.CreateDbContextForTenantAsync<ProjectsDbContext>(
            integrationEvent.TenantId,
            options => new ProjectsDbContext(options),
            cancellationToken);

        var memberships = await dbContext.ProjectUsers
            .Where(pu => pu.UserId == integrationEvent.UserId)
            .ToListAsync(cancellationToken);

        if (memberships.Count == 0)
        {
            return;
        }

        dbContext.ProjectUsers.RemoveRange(memberships);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation(
            "Removed {Count} project memberships for deleted UserId {UserId}, TenantId: {TenantId}",
            memberships.Count,
            integrationEvent.UserId,
            integrationEvent.TenantId);
    }
}

