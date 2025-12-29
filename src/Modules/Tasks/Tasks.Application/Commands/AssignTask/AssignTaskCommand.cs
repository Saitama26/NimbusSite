using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Tasks.Application.Abstractions;
using Tasks.Contracts.Events;
using Tasks.Domain.Errors;
using TaskStatus = Tasks.Domain.Enums.TaskStatus;

namespace Tasks.Application.Commands.AssignTask;

/// <summary>
/// Команда назначения задачи пользователю
/// </summary>
public sealed record AssignTaskCommand(
    Guid TaskId,
    int TenantId,
    Guid? AssignedToUserId) : ICommand;

/// <summary>
/// Обработчик назначения задачи пользователю
/// </summary>
internal sealed class AssignTaskCommandHandler : ICommandHandler<AssignTaskCommand>
{
    private readonly ITasksDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public AssignTaskCommandHandler(
        ITasksDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(AssignTaskCommand command, CancellationToken cancellationToken)
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

        if (task.AssignedToUserId == command.AssignedToUserId)
        {
            return Result.Success();
        }

        var previousUserId = task.AssignedToUserId;
        task.AssignedToUserId = command.AssignedToUserId;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new TaskAssignedEvent(
            task.Id,
            task.TenantId,
            task.ProjectId,
            command.AssignedToUserId,
            previousUserId,
            task.UpdatedAt);

        await _eventBus.PublishAsync(@event, cancellationToken);

        return Result.Success();
    }
}

