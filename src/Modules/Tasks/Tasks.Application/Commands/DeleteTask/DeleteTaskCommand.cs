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

namespace Tasks.Application.Commands.DeleteTask;

/// <summary>
/// Команда удаления задачи (soft delete через статус Deleted)
/// </summary>
public sealed record DeleteTaskCommand(Guid TaskId) : ICommand;

/// <summary>
/// Обработчик удаления задачи
/// </summary>
internal sealed class DeleteTaskCommandHandler : ICommandHandler<DeleteTaskCommand>
{
    private readonly ITaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public DeleteTaskCommandHandler(
        ITaskRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
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

        var oldStatus = task.Status;
        task.Status = TaskStatus.Deleted;
        task.UpdatedAt = DateTime.UtcNow;

        var events = new List<IDomainEvent>
        {
            new TaskStatusChangedEvent(
                task.Id,
                task.TenantId,
                task.ProjectId,
                (TaskStatusContract)(int)oldStatus,
                TaskStatusContract.Deleted,
                task.StartedAt,
                null,
                task.UpdatedAt),
            new TaskDeletedEvent(
                task.Id,
                task.TenantId,
                task.ProjectId,
                task.UpdatedAt)
        };

        await _repository.UpdateAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

