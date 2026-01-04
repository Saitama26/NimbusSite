using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Contracts.Events;
using Projects.Domain.Errors;
using Users.Contracts.Enums;
using Users.Domain.Enums;

namespace Projects.Application.Commands.ChangeUserRoleInProject;

/// <summary>
/// Команда изменения роли пользователя в проекте
/// </summary>
public sealed record ChangeUserRoleInProjectCommand(
    Guid ProjectId,
    int TenantId,
    Guid UserId,
    UserRole NewRole) : ICommand;

/// <summary>
/// Обработчик команды изменения роли пользователя в проекте
/// </summary>
internal sealed class ChangeUserRoleInProjectCommandHandler : ICommandHandler<ChangeUserRoleInProjectCommand>
{
    private readonly IProjectsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangeUserRoleInProjectCommandHandler(
        IProjectsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangeUserRoleInProjectCommand command, CancellationToken cancellationToken)
    {
        // Проверяем существование проекта с фильтрацией по TenantId
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId && p.TenantId == command.TenantId, cancellationToken);
        if (project == null)
        {
            return Result.Failure(ProjectErrors.NotFound(command.ProjectId));
        }

        // Находим связь ProjectUser
        var projectUser = await _dbContext.ProjectUsers
            .FirstOrDefaultAsync(pu => pu.ProjectId == command.ProjectId && pu.UserId == command.UserId, cancellationToken);
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new ProjectUserRoleChangedEvent(
            projectUser.Id,
            projectUser.ProjectId,
            project.TenantId,
            projectUser.UserId,
            (UserRoleContract)(int)oldRole,
            (UserRoleContract)(int)command.NewRole,
            DateTime.UtcNow);

        await _eventBus.PublishAsync(@event, cancellationToken);

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

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.NewRole)
            .IsInEnum().WithMessage("Invalid user role.");
    }
}

