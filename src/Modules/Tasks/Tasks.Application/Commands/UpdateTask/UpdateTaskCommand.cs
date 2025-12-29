using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Tasks.Application.Abstractions;
using Tasks.Contracts.Events;
using Tasks.Contracts.Enums;
using Tasks.Domain.Enums;
using Tasks.Domain.Errors;
using TaskStatus = Tasks.Domain.Enums.TaskStatus;

namespace Tasks.Application.Commands.UpdateTask;

/// <summary>
/// Команда обновления задачи
/// </summary>
public sealed record UpdateTaskCommand(
    Guid TaskId,
    int TenantId,
    string? Title = null,
    string? Description = null,
    TaskPriority? Priority = null,
    DateTime? DueDate = null) : ICommand;

/// <summary>
/// Обработчик обновления задачи
/// </summary>
internal sealed class UpdateTaskCommandHandler : ICommandHandler<UpdateTaskCommand>
{
    private readonly ITasksDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public UpdateTaskCommandHandler(
        ITasksDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == command.TaskId && t.TenantId == command.TenantId, cancellationToken);
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

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Публикация интеграционного события
            var @event = new TaskUpdatedEvent(
                task.Id,
                task.TenantId,
                task.ProjectId,
                task.Title,
                task.Description,
                (TaskPriorityContract?)(int?)task.Priority,
                task.DueDate,
                task.UpdatedAt);

            await _eventBus.PublishAsync(@event, cancellationToken);
        }

        return Result.Success();
    }
}

