using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Contracts.Events;
using Projects.Contracts.Enums;
using Projects.Domain.Enums;
using Projects.Domain.Errors;

namespace Projects.Application.Commands.DeleteProject;

/// <summary>
/// Команда удаления проекта (soft delete через статус Deleted).
/// </summary>
public sealed record DeleteProjectCommand(Guid ProjectId, int TenantId) : ICommand;

/// <summary>
/// Обработчик удаления проекта.
/// </summary>
internal sealed class DeleteProjectCommandHandler : ICommandHandler<DeleteProjectCommand>
{
    private readonly IProjectsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public DeleteProjectCommandHandler(
        IProjectsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId && p.TenantId == command.TenantId, cancellationToken);
        if (project == null)
        {
            return Result.Failure(ProjectErrors.NotFound(command.ProjectId));
        }

        if (project.Status == ProjectStatus.Deleted)
        {
            return Result.Failure(ProjectErrors.AlreadyDeleted);
        }

        var oldStatus = project.Status;
        project.Status = ProjectStatus.Deleted;
        project.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционных событий
        var statusChangedEvent = new ProjectStatusChangedEvent(
            project.Id,
            project.TenantId,
            (ProjectStatusContract)(int)oldStatus,
            ProjectStatusContract.Deleted,
            project.UpdatedAt);

        var deletedEvent = new ProjectDeletedEvent(
            project.Id,
            project.TenantId,
            project.UpdatedAt);

        await _eventBus.PublishAsync(statusChangedEvent, cancellationToken);
        await _eventBus.PublishAsync(deletedEvent, cancellationToken);

        return Result.Success();
    }
}

