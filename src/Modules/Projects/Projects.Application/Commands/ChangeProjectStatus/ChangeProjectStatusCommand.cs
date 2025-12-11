using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Projects.Application.Abstractions;
using Projects.Domain.Enums;
using Projects.Domain.Errors;
using Projects.Domain.Events;

namespace Projects.Application.Commands.ChangeProjectStatus;

/// <summary>
/// Команда изменения статуса проекта.
/// </summary>
/// <param name="ProjectId">Идентификатор проекта</param>
/// <param name="NewStatus">Новый статус проекта (Active, Archived, Deleted)</param>
public sealed record ChangeProjectStatusCommand(
    Guid ProjectId,
    ProjectStatus NewStatus) : ICommand;

/// <summary>
/// Обработчик изменения статуса проекта.
/// </summary>
internal sealed class ChangeProjectStatusCommandHandler : ICommandHandler<ChangeProjectStatusCommand>
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangeProjectStatusCommandHandler(
        IProjectRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangeProjectStatusCommand command, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result.Failure(ProjectErrors.NotFound(command.ProjectId));
        }

        if (project.Status == command.NewStatus)
        {
            return Result.Success();
        }

        var oldStatus = project.Status;
        project.Status = command.NewStatus;
        project.UpdatedAt = DateTime.UtcNow;

        var events = new List<IDomainEvent>
        {
            new ProjectStatusChangedEvent(project.Id, project.TenantId, oldStatus, command.NewStatus, project.UpdatedAt)
        };

        await _repository.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

