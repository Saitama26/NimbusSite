using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Tasks;
using Domain.Tasks.Errors;
using Domain.Tasks.Events;
using Domain.Tasks.ValueObjects;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Tasks.Commands.ChangeTaskStatus;

internal sealed class ChangeTaskStatusCommandHandler : ICommandHandler<ChangeTaskStatusCommand, Guid>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ChangeTaskStatusCommandHandler> _logger;

    public ChangeTaskStatusCommandHandler(
        ITaskRepository taskRepository,
        IApplicationDbContext context,
        ILogger<ChangeTaskStatusCommandHandler> logger)
    {
        _taskRepository = taskRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(ChangeTaskStatusCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Changing status for task {TaskId} to {NewStatus}",
            command.TaskId,
            command.NewStatus);

        var task = await _taskRepository.GetByIdAsync(command.TaskId, cancellationToken);

        if (task is null)
        {
            _logger.LogWarning(
                "Task {TaskId} not found",
                command.TaskId);
            return TaskErrors.NotFound(command.TaskId);
        }

        if (task.Status == command.NewStatus)
        {
            _logger.LogInformation(
                "Task {TaskId} already has status {Status}",
                command.TaskId,
                command.NewStatus);
            return task.Id; // Статус уже установлен
        }

        // Проверка валидности перехода статуса
        if (task.Status == Status.Done && command.NewStatus != Status.Done)
        {
            _logger.LogWarning(
                "Attempted to change status of completed task {TaskId} from {OldStatus} to {NewStatus}",
                command.TaskId,
                task.Status,
                command.NewStatus);
            return TaskErrors.AlreadyCompleted();
        }

        var oldStatus = task.Status;
        task.Status = command.NewStatus;
        task.AddEvent(new TaskStatusChangedEvent(task.Id, oldStatus, command.NewStatus));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Task {TaskId} status changed from {OldStatus} to {NewStatus}",
            command.TaskId,
            oldStatus,
            command.NewStatus);

        return task.Id;
    }
}

