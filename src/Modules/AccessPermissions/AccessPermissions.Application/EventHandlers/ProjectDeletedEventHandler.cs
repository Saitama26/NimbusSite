using AccessPermissions.Application.Abstractions;
using AccessPermissions.Domain.Entities;
using Common.Application.Abstractions.Events;
using Contracts.Projects.Events;
using Microsoft.Extensions.Logging;

namespace AccessPermissions.Application.EventHandlers;

/// <summary>
/// Удаляет права, привязанные к удалённому проекту.
/// </summary>
internal sealed class ProjectDeletedEventHandler : IEventHandler<ProjectDeletedEvent>
{
    private readonly IAccessPermissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProjectDeletedEventHandler> _logger;

    public ProjectDeletedEventHandler(
        IAccessPermissionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ProjectDeletedEventHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ProjectDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteByProjectIdAsync(domainEvent.ProjectId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Removed {Count} permissions for deleted ProjectId {ProjectId}", deleted, domainEvent.ProjectId);
    }
}

