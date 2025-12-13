using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Tasks.Application.Abstractions;
using Tasks.Domain.Entities;
using Tasks.Domain.Enums;
using Contracts.Tasks;
using Tasks.Domain.Errors;
using Contracts.Tasks.Events;
using TaskStatus = Tasks.Domain.Enums.TaskStatus;
using DomainTask = Tasks.Domain.Entities.Task;

namespace Tasks.Application.Commands.CreateTask;

/// <summary>
/// Команда создания задачи
/// </summary>
public sealed record CreateTaskCommand(
    Guid TenantId,
    Guid ProjectId,
    string Title,
    Guid CreatedByUserId,
    TaskPriority Priority = TaskPriority.Normal,
    string? Description = null,
    Guid? AssignedToUserId = null,
    DateTime? DueDate = null) : ICommand<CreateTaskResponse>;

/// <summary>
/// Ответ при создании задачи
/// </summary>
public sealed record CreateTaskResponse(Guid TaskId);

/// <summary>
/// Обработчик создания задачи
/// </summary>
internal sealed class CreateTaskCommandHandler : ICommandHandler<CreateTaskCommand, CreateTaskResponse>
{
    private readonly ITaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public CreateTaskCommandHandler(
        ITaskRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<CreateTaskResponse>> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var title = command.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result<CreateTaskResponse>.Failure(TaskErrors.TitleEmpty);
        }

        if (title.Length > 500)
        {
            return Result<CreateTaskResponse>.Failure(TaskErrors.TitleTooLong);
        }

        if (command.DueDate.HasValue && command.DueDate.Value < DateTime.UtcNow.Date)
        {
            return Result<CreateTaskResponse>.Failure(TaskErrors.InvalidDueDate);
        }

        var task = new DomainTask
        {
            TenantId = command.TenantId,
            ProjectId = command.ProjectId,
            Title = title,
            Description = command.Description?.Trim(),
            Status = TaskStatus.New,
            Priority = command.Priority,
            AssignedToUserId = command.AssignedToUserId,
            CreatedByUserId = command.CreatedByUserId,
            DueDate = command.DueDate
        };

        var events = new List<IDomainEvent>
        {
            new TaskCreatedEvent(
                task.Id,
                task.TenantId,
                task.ProjectId,
                task.Title,
                (TaskStatusContract)(int)task.Status,
                (TaskPriorityContract)(int)task.Priority,
                task.CreatedByUserId,
                task.Description,
                task.AssignedToUserId,
                task.DueDate,
                task.CreatedAt)
        };

        await _repository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

        return Result<CreateTaskResponse>.Success(new CreateTaskResponse(task.Id));
    }
}

