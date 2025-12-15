using AccessPermissions.Application.Abstractions;
using AccessPermissions.Domain.Entities;
using Common.Application.Abstractions.Events;
using Contracts.Tasks.Events;
using Microsoft.Extensions.Logging;

namespace AccessPermissions.Application.EventHandlers;

/// <summary>
/// Удаляет права, привязанные к удалённой задаче.
/// </summary>
internal sealed class TaskDeletedEventHandler : IEventHandler<TaskDeletedEvent>
{
    private readonly IAccessPermissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TaskDeletedEventHandler> _logger;

    public TaskDeletedEventHandler(
        IAccessPermissionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<TaskDeletedEventHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(TaskDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteByTaskIdAsync(domainEvent.TaskId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Removed {Count} permissions for deleted TaskId {TaskId}", deleted, domainEvent.TaskId);
    }
}

