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

namespace Tasks.Application.Commands.DeleteTask;

/// <summary>
/// Команда удаления задачи (soft delete через статус Deleted)
/// </summary>
public sealed record DeleteTaskCommand(Guid TaskId, int TenantId) : ICommand;

/// <summary>
/// Обработчик удаления задачи
/// </summary>
internal sealed class DeleteTaskCommandHandler : ICommandHandler<DeleteTaskCommand>
{
    private readonly ITasksDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public DeleteTaskCommandHandler(
        ITasksDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
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

        var oldStatus = task.Status;
        task.Status = TaskStatus.Deleted;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционных событий
        var statusChangedEvent = new TaskStatusChangedEvent(
            task.Id,
            task.TenantId,
            task.ProjectId,
            (TaskStatusContract)(int)oldStatus,
            TaskStatusContract.Deleted,
            task.StartedAt,
            null,
            task.UpdatedAt);

        var deletedEvent = new TaskDeletedEvent(
            task.Id,
            task.TenantId,
            task.ProjectId,
            task.UpdatedAt);

        await _eventBus.PublishAsync(statusChangedEvent, cancellationToken);
        await _eventBus.PublishAsync(deletedEvent, cancellationToken);

        return Result.Success();
    }
}

