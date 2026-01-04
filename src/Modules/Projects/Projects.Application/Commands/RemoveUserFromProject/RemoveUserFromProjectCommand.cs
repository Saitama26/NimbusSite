using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Contracts.Events;
using Projects.Domain.Errors;

namespace Projects.Application.Commands.RemoveUserFromProject;

/// <summary>
/// Команда удаления пользователя из проекта
/// </summary>
public sealed record RemoveUserFromProjectCommand(
    Guid ProjectId,
    int TenantId,
    Guid UserId) : ICommand;

/// <summary>
/// Обработчик команды удаления пользователя из проекта
/// </summary>
internal sealed class RemoveUserFromProjectCommandHandler : ICommandHandler<RemoveUserFromProjectCommand>
{
    private readonly IProjectsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public RemoveUserFromProjectCommandHandler(
        IProjectsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(RemoveUserFromProjectCommand command, CancellationToken cancellationToken)
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

        _dbContext.ProjectUsers.Remove(projectUser);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new ProjectUserRemovedEvent(
            projectUser.Id,
            projectUser.ProjectId,
            project.TenantId,
            projectUser.UserId,
            DateTime.UtcNow);

        await _eventBus.PublishAsync(@event, cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Валидатор команды удаления пользователя из проекта
/// </summary>
internal sealed class RemoveUserFromProjectCommandValidator : AbstractValidator<RemoveUserFromProjectCommand>
{
    public RemoveUserFromProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

