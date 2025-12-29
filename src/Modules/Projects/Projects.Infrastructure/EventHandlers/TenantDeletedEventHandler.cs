using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Projects.Domain.Enums;
using Tenants.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Projects.Infrastructure;

namespace Projects.Infrastructure.EventHandlers;

/// <summary>
/// Помечает проекты удалённого тенанта как удалённые (soft delete)
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

    public async Task Handle(TenantDeletedEvent integrationEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling TenantDeletedEvent for TenantInt: {TenantInt}",
            integrationEvent.TenantInt);

        // Создаем DbContext для тенанта из события
        using var dbContext = await _dbContextFactory.CreateDbContextForTenantAsync<ProjectsDbContext>(
            integrationEvent.TenantInt,
            options => new ProjectsDbContext(options),
            cancellationToken);

        var projects = await dbContext.Projects
            .Where(p => p.TenantId == integrationEvent.TenantInt)
            .ToListAsync(cancellationToken);

        if (projects.Count == 0)
        {
            return;
        }

        foreach (var project in projects)
        {
            project.Status = ProjectStatus.Deleted;
            project.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation(
            "Marked {Count} projects as Deleted for TenantInt {TenantInt}",
            projects.Count,
            integrationEvent.TenantInt);
    }
}

