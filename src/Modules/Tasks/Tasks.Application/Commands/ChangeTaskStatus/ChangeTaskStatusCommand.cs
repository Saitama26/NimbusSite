using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Tasks.Application.Abstractions;
using Tasks.Domain.Enums;
using Contracts.Tasks;
using Tasks.Domain.Errors;
using Contracts.Tasks.Events;
using TaskStatus = Tasks.Domain.Enums.TaskStatus;

namespace Tasks.Application.Commands.ChangeTaskStatus;

/// <summary>
/// Команда изменения статуса задачи
/// </summary>
public sealed record ChangeTaskStatusCommand(
    Guid TaskId,
    TaskStatus NewStatus) : ICommand;

/// <summary>
/// Обработчик изменения статуса задачи
/// </summary>
internal sealed class ChangeTaskStatusCommandHandler : ICommandHandler<ChangeTaskStatusCommand>
{
    private readonly ITaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangeTaskStatusCommandHandler(
        ITaskRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangeTaskStatusCommand command, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(command.TaskId, cancellationToken);
        if (task == null)
        {
            return Result.Failure(TaskErrors.NotFound(command.TaskId));
        }

        if (task.Status == command.NewStatus)
        {
            return Result.Success();
        }

        var oldStatus = task.Status;
        var now = DateTime.UtcNow;

        task.Status = command.NewStatus;
        task.UpdatedAt = now;

        // Обновляем даты в зависимости от статуса
        if (command.NewStatus == TaskStatus.InProgress && !task.StartedAt.HasValue)
        {
            task.StartedAt = now;
        }

        if (command.NewStatus == TaskStatus.Completed)
        {
            task.CompletedAt = now;
        }
        else if (oldStatus == TaskStatus.Completed && command.NewStatus != TaskStatus.Completed)
        {
            task.CompletedAt = null;
        }

        var events = new List<IDomainEvent>
        {
            new TaskStatusChangedEvent(
                task.Id,
                task.TenantId,
                task.ProjectId,
                (TaskStatusContract)(int)oldStatus,
                (TaskStatusContract)(int)command.NewStatus,
                task.StartedAt,
                task.CompletedAt,
                task.UpdatedAt)
        };

        await _repository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

