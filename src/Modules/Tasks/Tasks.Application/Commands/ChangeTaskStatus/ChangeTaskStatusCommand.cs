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

namespace Tasks.Application.Commands.ChangeTaskStatus;

/// <summary>
/// Команда изменения статуса задачи
/// </summary>
public sealed record ChangeTaskStatusCommand(
    Guid TaskId,
    int TenantId,
    TaskStatus NewStatus) : ICommand;

/// <summary>
/// Обработчик изменения статуса задачи
/// </summary>
internal sealed class ChangeTaskStatusCommandHandler : ICommandHandler<ChangeTaskStatusCommand>
{
    private readonly ITasksDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangeTaskStatusCommandHandler(
        ITasksDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangeTaskStatusCommand command, CancellationToken cancellationToken)
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new TaskStatusChangedEvent(
            task.Id,
            task.TenantId,
            task.ProjectId,
            (TaskStatusContract)(int)oldStatus,
            (TaskStatusContract)(int)command.NewStatus,
            task.StartedAt,
            task.CompletedAt,
            task.UpdatedAt);

        await _eventBus.PublishAsync(@event, cancellationToken);

        return Result.Success();
    }
}

