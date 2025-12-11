using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Projects.Application.Abstractions;
using Projects.Domain.Enums;
using Projects.Domain.Errors;
using Projects.Domain.Events;

namespace Projects.Application.Commands.DeleteProject;

/// <summary>
/// Команда удаления проекта (soft delete через статус Deleted).
/// </summary>
public sealed record DeleteProjectCommand(Guid ProjectId) : ICommand;

/// <summary>
/// Обработчик удаления проекта.
/// </summary>
internal sealed class DeleteProjectCommandHandler : ICommandHandler<DeleteProjectCommand>
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public DeleteProjectCommandHandler(
        IProjectRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result.Failure(ProjectErrors.NotFound(command.ProjectId));
        }

        if (project.Status == ProjectStatus.Deleted)
        {
            return Result.Failure(ProjectErrors.AlreadyDeleted);
        }

        var oldStatus = project.Status;
        project.Status = ProjectStatus.Deleted;
        project.UpdatedAt = DateTime.UtcNow;

        var events = new List<IDomainEvent>
        {
            new ProjectStatusChangedEvent(project.Id, project.TenantId, oldStatus, ProjectStatus.Deleted, project.UpdatedAt),
            new ProjectDeletedEvent(project.Id, project.TenantId, project.UpdatedAt)
        };

        await _repository.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

