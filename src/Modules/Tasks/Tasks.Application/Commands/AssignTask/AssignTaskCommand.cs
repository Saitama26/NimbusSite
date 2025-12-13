using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Tasks.Application.Abstractions;
using Tasks.Domain.Errors;
using Contracts.Tasks.Events;
using TaskStatus = Tasks.Domain.Enums.TaskStatus;

namespace Tasks.Application.Commands.AssignTask;

/// <summary>
/// Команда назначения задачи пользователю
/// </summary>
public sealed record AssignTaskCommand(
    Guid TaskId,
    Guid? AssignedToUserId) : ICommand;

/// <summary>
/// Обработчик назначения задачи пользователю
/// </summary>
internal sealed class AssignTaskCommandHandler : ICommandHandler<AssignTaskCommand>
{
    private readonly ITaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public AssignTaskCommandHandler(
        ITaskRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(AssignTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(command.TaskId, cancellationToken);
        if (task == null)
        {
            return Result.Failure(TaskErrors.NotFound(command.TaskId));
        }

        if (task.Status == TaskStatus.Deleted)
        {
            return Result.Failure(TaskErrors.AlreadyDeleted);
        }

        if (task.AssignedToUserId == command.AssignedToUserId)
        {
            return Result.Success();
        }

        var previousUserId = task.AssignedToUserId;
        task.AssignedToUserId = command.AssignedToUserId;
        task.UpdatedAt = DateTime.UtcNow;

        var events = new List<IDomainEvent>
        {
            new TaskAssignedEvent(
                task.Id,
                task.TenantId,
                task.ProjectId,
                command.AssignedToUserId,
                previousUserId,
                task.UpdatedAt)
        };

        await _repository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

