using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Projects.Application.Abstractions;
using Projects.Domain.Errors;
using Contracts.Projects.Events;

namespace Projects.Application.Commands.UpdateProject;

/// <summary>
/// Команда обновления проекта.
/// </summary>
/// <param name="ProjectId">Идентификатор проекта</param>
/// <param name="Name">Новое название проекта (необязательное, обновляется только если указано)</param>
/// <param name="Description">Новое описание проекта (необязательное, может быть пустым для очистки)</param>
public sealed record UpdateProjectCommand(
    Guid ProjectId,
    string? Name = null,
    string? Description = null) : ICommand;

/// <summary>
/// Обработчик обновления проекта.
/// </summary>
internal sealed class UpdateProjectCommandHandler : ICommandHandler<UpdateProjectCommand>
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public UpdateProjectCommandHandler(
        IProjectRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result.Failure(ProjectErrors.NotFound(command.ProjectId));
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
            var events = new List<IDomainEvent>
            {
                new ProjectUpdatedEvent(
                    project.Id,
                    project.TenantId,
                    project.Name,
                    project.Description,
                    project.UpdatedAt)
            };

            await _repository.UpdateAsync(project, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _eventBus.PublishAsync(events, cancellationToken);
        }

        return Result.Success();
    }
}

