using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Projects.Application.Abstractions;
using Projects.Domain.Errors;
using Users.Domain.Enums;
using Contracts.Projects.Events;

namespace Projects.Application.Commands.ChangeUserRoleInProject;

/// <summary>
/// Команда изменения роли пользователя в проекте
/// </summary>
public sealed record ChangeUserRoleInProjectCommand(
    Guid ProjectId,
    Guid UserId,
    UserRole NewRole) : ICommand;

/// <summary>
/// Обработчик команды изменения роли пользователя в проекте
/// </summary>
internal sealed class ChangeUserRoleInProjectCommandHandler : ICommandHandler<ChangeUserRoleInProjectCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectUserRepository _projectUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangeUserRoleInProjectCommandHandler(
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

    public async Task<Result> Handle(ChangeUserRoleInProjectCommand command, CancellationToken cancellationToken)
    {
        // Проверяем существование проекта
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result.Failure(ProjectErrors.NotFound(command.ProjectId));
        }

        // Находим связь ProjectUser
        var projectUser = await _projectUserRepository.GetByProjectAndUserAsync(command.ProjectId, command.UserId, cancellationToken);
        if (projectUser == null)
        {
            return Result.Failure(ProjectErrors.UserNotInProject(command.UserId, command.ProjectId));
        }

        // Проверяем, изменилась ли роль
        if (projectUser.Role == command.NewRole)
        {
            return Result.Success(); // Роль не изменилась, ничего не делаем
        }

        var oldRole = projectUser.Role;
        projectUser.Role = command.NewRole;

        var events = new List<IDomainEvent>
        {
            new ProjectUserRoleChangedEvent(
                projectUser.Id,
                projectUser.ProjectId,
                project.TenantId,
                projectUser.UserId,
                (Contracts.Users.UserRoleContract)(int)oldRole,
                (Contracts.Users.UserRoleContract)(int)command.NewRole,
                DateTime.UtcNow)
        };

        await _projectUserRepository.UpdateAsync(projectUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Валидатор команды изменения роли пользователя в проекте
/// </summary>
internal sealed class ChangeUserRoleInProjectCommandValidator : AbstractValidator<ChangeUserRoleInProjectCommand>
{
    public ChangeUserRoleInProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.NewRole)
            .IsInEnum().WithMessage("Invalid user role.");
    }
}

