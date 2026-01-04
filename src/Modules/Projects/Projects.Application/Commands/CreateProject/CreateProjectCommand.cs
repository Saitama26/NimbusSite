using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Application.Abstractions.Views;
using Projects.Contracts.Events;
using Projects.Contracts.Enums;
using Projects.Domain.Entities;
using Projects.Domain.Enums;
using Projects.Domain.Errors;

namespace Projects.Application.Commands.CreateProject;

/// <summary>
/// Команда создания проекта
/// Проект создается в tenant-специфичной БД (таблицы в корне базы данных без схем)
/// </summary>
public sealed record CreateProjectCommand(
    int TenantId,
    Guid CreatedByUserId,
    string Name,
    string? Description = null) : ICommand<CreateProjectResponse>;

/// <summary>
/// Ответ при создании проекта.
/// </summary>
/// <param name="ProjectId">Идентификатор созданного проекта</param>
public sealed record CreateProjectResponse(Guid ProjectId);

/// <summary>
/// Обработчик создания проекта
/// </summary>
internal sealed class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, CreateProjectResponse>
{
    private readonly IProjectsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly IUserViewRepository _userViewRepository;

    public CreateProjectCommandHandler(
        IProjectsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        IUserViewRepository userViewRepository)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _userViewRepository = userViewRepository;
    }

    public async Task<Result<CreateProjectResponse>> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var name = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<CreateProjectResponse>.Failure(ProjectErrors.NameEmpty);
        }

        // Проверяем существование пользователя через View
        var userExists = await _userViewRepository.ExistsAsync(command.CreatedByUserId, cancellationToken);
        if (!userExists)
        {
            return Result<CreateProjectResponse>.Failure(ProjectErrors.UserNotFound(command.CreatedByUserId));
        }

        // Проверяем уникальность имени проекта в рамках тенанта
        var exists = await _dbContext.Projects
            .AnyAsync(p => p.TenantId == command.TenantId && p.Name == name, cancellationToken);
        if (exists)
        {
            return Result<CreateProjectResponse>.Failure(ProjectErrors.NameAlreadyExists(name));
        }

        // Создаем проект
        var project = new Project
        {
            TenantId = command.TenantId,
            Name = name,
            Description = command.Description?.Trim(),
            Status = ProjectStatus.Active,
        };

        _dbContext.Projects.Add(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new ProjectCreatedEvent(
            project.Id,
            project.TenantId,
            command.CreatedByUserId,
            project.Name,
            (ProjectStatusContract)(int)project.Status,
            project.Description,
            project.CreatedAt);

        await _eventBus.PublishAsync(@event, cancellationToken);

        return Result<CreateProjectResponse>.Success(new CreateProjectResponse(project.Id));
    }
}

