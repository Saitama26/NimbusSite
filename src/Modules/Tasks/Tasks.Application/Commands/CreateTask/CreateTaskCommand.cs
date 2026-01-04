using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Tasks.Application.Abstractions;
using Tasks.Application.Abstractions.Views;
using Tasks.Contracts.Events;
using Tasks.Contracts.Enums;
using Tasks.Domain.Entities;
using Tasks.Domain.Enums;
using Tasks.Domain.Errors;
using TaskStatus = Tasks.Domain.Enums.TaskStatus;
using DomainTask = Tasks.Domain.Entities.Task;

namespace Tasks.Application.Commands.CreateTask;

/// <summary>
/// Команда создания задачи
/// Задача создается в tenant-специфичной БД со схемой Tasks
/// </summary>
public sealed record CreateTaskCommand(
    int TenantId,
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
    private readonly ITasksDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly IProjectViewRepository _projectViewRepository;

    public CreateTaskCommandHandler(
        ITasksDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        IProjectViewRepository projectViewRepository)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _projectViewRepository = projectViewRepository;
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

        // Проверяем существование проекта через View
        var projectExists = await _projectViewRepository.ExistsAsync(command.ProjectId, cancellationToken);
        if (!projectExists)
        {
            return Result<CreateTaskResponse>.Failure(TaskErrors.ProjectNotFound(command.ProjectId));
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

        _dbContext.Tasks.Add(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new TaskCreatedEvent(
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
            task.CreatedAt);

        await _eventBus.PublishAsync(@event, cancellationToken);

        return Result<CreateTaskResponse>.Success(new CreateTaskResponse(task.Id));
    }
}

