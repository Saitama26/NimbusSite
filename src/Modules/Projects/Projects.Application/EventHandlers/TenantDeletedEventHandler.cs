using Common.Application.Abstractions.Events;
using Contracts.Tenants.Events;
using Projects.Application.Abstractions;
using Projects.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Projects.Application.EventHandlers;

/// <summary>
/// Помечает проекты удалённого тенанта как удалённые.
/// </summary>
internal sealed class TenantDeletedEventHandler : IEventHandler<TenantDeletedEvent>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TenantDeletedEventHandler> _logger;

    public TenantDeletedEventHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        ILogger<TenantDeletedEventHandler> logger)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(TenantDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        var projects = await _projectRepository.GetByTenantIdAsync(domainEvent.TenantId, cancellationToken);
        var list = projects.ToList();
        if (list.Count == 0)
        {
            return;
        }

        foreach (var p in list)
        {
            // soft-delete через статус или удаление записи; здесь ставим статус Deleted (если есть)
            p.Status = Projects.Domain.Enums.ProjectStatus.Deleted;
            await _projectRepository.UpdateAsync(p, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Marked {Count} projects as Deleted for TenantId {TenantId}", list.Count, domainEvent.TenantId);
    }
}

