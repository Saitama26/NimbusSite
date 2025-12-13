using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Projects.Application.Abstractions;
using Projects.Domain.Entities;
using Projects.Domain.Errors;
using Users.Domain.Enums;
using Contracts.Projects.Events;

namespace Projects.Application.Commands.AddUserToProject;

/// <summary>
/// Команда добавления пользователя в проект
/// </summary>
public sealed record AddUserToProjectCommand(
    Guid ProjectId,
    Guid UserId,
    UserRole Role) : ICommand<AddUserToProjectResponse>;

/// <summary>
/// Ответ при добавлении пользователя в проект
/// </summary>
public sealed record AddUserToProjectResponse(Guid ProjectUserId);

/// <summary>
/// Обработчик команды добавления пользователя в проект
/// </summary>
internal sealed class AddUserToProjectCommandHandler : ICommandHandler<AddUserToProjectCommand, AddUserToProjectResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectUserRepository _projectUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public AddUserToProjectCommandHandler(
        IProjectRepository projectRepository,
        IProjectUserRepository projectUserRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _projectRepository = projectRepository;
        _projectUserRepository = projectUserRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<AddUserToProjectResponse>> Handle(AddUserToProjectCommand command, CancellationToken cancellationToken)
    {
        // Проверяем существование проекта
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<AddUserToProjectResponse>.Failure(ProjectErrors.NotFound(command.ProjectId));
        }

        // Проверяем, не добавлен ли уже пользователь в проект
        var exists = await _projectUserRepository.ExistsAsync(command.ProjectId, command.UserId, cancellationToken);
        if (exists)
        {
            return Result<AddUserToProjectResponse>.Failure(ProjectErrors.UserAlreadyInProject(command.UserId, command.ProjectId));
        }

        // Создаем связь ProjectUser
        var projectUser = new ProjectUser
        {
            ProjectId = command.ProjectId,
            UserId = command.UserId,
            Role = command.Role,
            JoinedAt = DateTime.UtcNow
        };

        var events = new List<IDomainEvent>
        {
            new ProjectUserCreatedEvent(
                projectUser.Id,
                projectUser.ProjectId,
                project.TenantId,
                projectUser.UserId,
                (Contracts.Users.UserRoleContract)(int)projectUser.Role,
                projectUser.JoinedAt,
                projectUser.CreatedAt)
        };

        await _projectUserRepository.AddAsync(projectUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

        return Result<AddUserToProjectResponse>.Success(new AddUserToProjectResponse(projectUser.Id));
    }
}

/// <summary>
/// Валидатор команды добавления пользователя в проект
/// </summary>
internal sealed class AddUserToProjectCommandValidator : AbstractValidator<AddUserToProjectCommand>
{
    public AddUserToProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role.");
    }
}

