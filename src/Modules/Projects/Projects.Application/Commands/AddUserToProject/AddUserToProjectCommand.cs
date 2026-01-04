using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Contracts.Events;
using Projects.Domain.Entities;
using Projects.Domain.Errors;
using Users.Contracts.Enums;
using Users.Domain.Enums;

namespace Projects.Application.Commands.AddUserToProject;

/// <summary>
/// Команда добавления пользователя в проект
/// </summary>
public sealed record AddUserToProjectCommand(
    Guid ProjectId,
    int TenantId,
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
    private readonly IProjectsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public AddUserToProjectCommandHandler(
        IProjectsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<AddUserToProjectResponse>> Handle(AddUserToProjectCommand command, CancellationToken cancellationToken)
    {
        // Проверяем существование проекта с фильтрацией по TenantId
        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId && p.TenantId == command.TenantId, cancellationToken);
        if (project == null)
        {
            return Result<AddUserToProjectResponse>.Failure(ProjectErrors.NotFound(command.ProjectId));
        }

        // Проверяем, не добавлен ли уже пользователь в проект
        var exists = await _dbContext.ProjectUsers
            .AnyAsync(pu => pu.ProjectId == command.ProjectId && pu.UserId == command.UserId, cancellationToken);
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

        _dbContext.ProjectUsers.Add(projectUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new ProjectUserCreatedEvent(
            projectUser.Id,
            projectUser.ProjectId,
            project.TenantId,
            projectUser.UserId,
            (UserRoleContract)(int)projectUser.Role,
            projectUser.JoinedAt,
            projectUser.CreatedAt);

        await _eventBus.PublishAsync(@event, cancellationToken);

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

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role.");
    }
}

