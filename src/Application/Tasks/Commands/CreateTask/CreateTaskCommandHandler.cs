using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Projects;
using Domain.Tasks;
using Domain.Tasks.Errors;
using Domain.Tasks.Events;
using Domain.Tasks.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Tasks.Commands.CreateTask;

internal sealed class CreateTaskCommandHandler : ICommandHandler<CreateTaskCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<CreateTaskCommandHandler> _logger;

    public CreateTaskCommandHandler(
        IApplicationDbContext context,
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        ILogger<CreateTaskCommandHandler> logger)
    {
        _context = context;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating task '{TaskTitle}' for project {ProjectId}",
            command.Title,
            command.ProjectId);

        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project is null)
        {
            _logger.LogWarning(
                "Project {ProjectId} not found when creating task",
                command.ProjectId);
            return TaskErrors.NotFound(command.ProjectId);
        }

        // Проверяем, существует ли задача с таким названием в проекте
        var existingTasks = await _taskRepository.GetByProjectIdAsync(command.ProjectId, cancellationToken);
        if (existingTasks.Any(t => t.Title == command.Title))
        {
            _logger.LogWarning(
                "Task with title '{TaskTitle}' already exists in project {ProjectId}",
                command.Title,
                command.ProjectId);
            return TaskErrors.AlreadyExists(command.Title);
        }

        // Проверяем, что DueDate не в прошлом
        if (command.DueDate.HasValue && command.DueDate.Value < DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Task '{TaskTitle}' has deadline in the past: {DueDate}",
                command.Title,
                command.DueDate.Value);
            return TaskErrors.DeadlineInPast();
        }

        var taskId = Guid.NewGuid();
        var task = new ProjectTask
        {
            Id = taskId,
            TenantId = command.TenantId,
            ProjectId = command.ProjectId,
            Title = command.Title,
            Description = command.Description,
            AssignedUserId = command.AssignedUserId,
            DueDate = command.DueDate,
            Priority = command.Priority,
            Status = Status.New,
            CreatedAt = DateTime.UtcNow
        };

        task.AddEvent(new TaskCreatedEvent(task.Id, task.Title));
        task.AddEvent(new TaskAssignedEvent(task.Id, task.AssignedUserId));

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Task {TaskId} '{TaskTitle}' created successfully for project {ProjectId}",
            taskId,
            command.Title,
            command.ProjectId);

        return task.Id;
    }
}

