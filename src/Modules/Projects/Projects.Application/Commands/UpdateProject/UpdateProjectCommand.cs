using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Contracts.Events;
using Projects.Domain.Errors;

namespace Projects.Application.Commands.UpdateProject;

/// <summary>
/// Команда обновления проекта.
/// </summary>
/// <param name="ProjectId">Идентификатор проекта</param>
/// <param name="TenantId">Идентификатор тенанта</param>
/// <param name="Name">Новое название проекта (необязательное, обновляется только если указано)</param>
/// <param name="Description">Новое описание проекта (необязательное, может быть пустым для очистки)</param>
public sealed record UpdateProjectCommand(
    Guid ProjectId,
    int TenantId,
    string? Name = null,
    string? Description = null) : ICommand;

/// <summary>
/// Обработчик обновления проекта.
/// </summary>
internal sealed class UpdateProjectCommandHandler : ICommandHandler<UpdateProjectCommand>
{
    private readonly IProjectsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public UpdateProjectCommandHandler(
        IProjectsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId && p.TenantId == command.TenantId, cancellationToken);
        if (project == null)
        {
            return Result.Failure(ProjectErrors.NotFound(command.ProjectId));
        }

        if (project.Status == Projects.Domain.Enums.ProjectStatus.Deleted)
        {
            return Result.Failure(ProjectErrors.AlreadyDeleted);
        }

        var hasChanges = false;

        if (!string.IsNullOrWhiteSpace(command.Name) && project.Name != command.Name.Trim())
        {
            project.Name = command.Name.Trim();
            hasChanges = true;
        }

        if (command.Description != null && project.Description != command.Description.Trim())
        {
            project.Description = command.Description.Trim();
            hasChanges = true;
        }

        if (hasChanges)
        {
            project.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Публикация интеграционного события
            var @event = new ProjectUpdatedEvent(
                project.Id,
                project.TenantId,
                project.Name,
                project.Description,
                project.UpdatedAt);

            await _eventBus.PublishAsync(@event, cancellationToken);
        }

        return Result.Success();
    }
}

