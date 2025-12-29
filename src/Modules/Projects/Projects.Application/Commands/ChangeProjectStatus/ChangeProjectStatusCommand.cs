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

namespace Projects.Application.Commands.ChangeProjectStatus;

/// <summary>
/// Команда изменения статуса проекта.
/// </summary>
/// <param name="ProjectId">Идентификатор проекта</param>
/// <param name="TenantId">Идентификатор тенанта</param>
/// <param name="NewStatus">Новый статус проекта (Active, Archived, Deleted)</param>
public sealed record ChangeProjectStatusCommand(
    Guid ProjectId,
    int TenantId,
    ProjectStatus NewStatus) : ICommand;

/// <summary>
/// Обработчик изменения статуса проекта.
/// </summary>
internal sealed class ChangeProjectStatusCommandHandler : ICommandHandler<ChangeProjectStatusCommand>
{
    private readonly IProjectsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangeProjectStatusCommandHandler(
        IProjectsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangeProjectStatusCommand command, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId && p.TenantId == command.TenantId, cancellationToken);
        if (project == null)
        {
            return Result.Failure(ProjectErrors.NotFound(command.ProjectId));
        }

        // Если проект уже удален, не позволяем изменять статус
        if (project.Status == ProjectStatus.Deleted)
        {
            return Result.Failure(ProjectErrors.AlreadyDeleted);
        }

        if (project.Status == command.NewStatus)
        {
            return Result.Success();
        }

        var oldStatus = project.Status;
        project.Status = command.NewStatus;
        project.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new ProjectStatusChangedEvent(
            project.Id,
            project.TenantId,
            (ProjectStatusContract)(int)oldStatus,
            (ProjectStatusContract)(int)command.NewStatus,
            project.UpdatedAt);

        await _eventBus.PublishAsync(@event, cancellationToken);

        return Result.Success();
    }
}

