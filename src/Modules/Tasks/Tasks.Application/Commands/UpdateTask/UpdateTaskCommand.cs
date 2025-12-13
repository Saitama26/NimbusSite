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

namespace Tasks.Application.Commands.UpdateTask;

/// <summary>
/// Команда обновления задачи
/// </summary>
public sealed record UpdateTaskCommand(
    Guid TaskId,
    string? Title = null,
    string? Description = null,
    TaskPriority? Priority = null,
    DateTime? DueDate = null) : ICommand;

/// <summary>
/// Обработчик обновления задачи
/// </summary>
internal sealed class UpdateTaskCommandHandler : ICommandHandler<UpdateTaskCommand>
{
    private readonly ITaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public UpdateTaskCommandHandler(
        ITaskRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
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

        var hasChanges = false;

        if (!string.IsNullOrWhiteSpace(command.Title))
        {
            var title = command.Title.Trim();
            if (title.Length > 500)
            {
                return Result.Failure(TaskErrors.TitleTooLong);
            }
            if (task.Title != title)
            {
                task.Title = title;
                hasChanges = true;
            }
        }

        if (command.Description != null && task.Description != command.Description.Trim())
        {
            task.Description = command.Description.Trim();
            hasChanges = true;
        }

        if (command.Priority.HasValue && task.Priority != command.Priority.Value)
        {
            task.Priority = command.Priority.Value;
            hasChanges = true;
        }

        if (command.DueDate != task.DueDate)
        {
            if (command.DueDate.HasValue && command.DueDate.Value < DateTime.UtcNow.Date)
            {
                return Result.Failure(TaskErrors.InvalidDueDate);
            }
            task.DueDate = command.DueDate;
            hasChanges = true;
        }

        if (hasChanges)
        {
            task.UpdatedAt = DateTime.UtcNow;
            var events = new List<IDomainEvent>
            {
                new TaskUpdatedEvent(
                    task.Id,
                    task.TenantId,
                    task.ProjectId,
                    task.Title,
                    (TaskPriorityContract)(int)task.Priority,
                    task.Description,
                    task.DueDate,
                    task.UpdatedAt)
            };

            await _repository.UpdateAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _eventBus.PublishAsync(events, cancellationToken);
        }

        return Result.Success();
    }
}

