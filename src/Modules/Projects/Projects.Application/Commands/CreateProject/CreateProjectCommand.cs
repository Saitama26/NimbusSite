using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Projects.Application.Abstractions;
using Projects.Domain.Entities;
using Projects.Domain.Enums;
using Projects.Domain.Errors;
using Projects.Domain.Events;

namespace Projects.Application.Commands.CreateProject;

/// <summary>
/// Команда создания проекта.
/// </summary>
/// <param name="TenantId">Идентификатор тенанта</param>
/// <param name="Name">Название проекта (обязательное, должно быть уникальным в рамках тенанта)</param>
/// <param name="Description">Описание проекта (необязательное)</param>
public sealed record CreateProjectCommand(
    Guid TenantId,
    string Name,
    string? Description = null) : ICommand<CreateProjectResponse>;

/// <summary>
/// Ответ при создании проекта.
/// </summary>
/// <param name="ProjectId">Идентификатор созданного проекта</param>
public sealed record CreateProjectResponse(Guid ProjectId);

/// <summary>
/// Обработчик создания проекта.
/// </summary>
internal sealed class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, CreateProjectResponse>
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public CreateProjectCommandHandler(
        IProjectRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<CreateProjectResponse>> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var name = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<CreateProjectResponse>.Failure(ProjectErrors.NameEmpty);
        }

        var exists = await _repository.ExistsByNameAsync(command.TenantId, name, cancellationToken);
        if (exists)
        {
            return Result<CreateProjectResponse>.Failure(ProjectErrors.NameAlreadyExists(name));
        }

        var project = new Project
        {
            TenantId = command.TenantId,
            Name = name,
            Description = command.Description?.Trim(),
            Status = ProjectStatus.Active,
        };

        var events = new List<IDomainEvent>
        {
            new ProjectCreatedEvent(
                project.Id,
                project.TenantId,
                project.Name,
                project.Status,
                project.Description,
                project.CreatedAt)
        };

        await _repository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

        return Result<CreateProjectResponse>.Success(new CreateProjectResponse(project.Id));
    }
}

