using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Projects.Application.Abstractions;
using Projects.Domain.Errors;
using Contracts.Projects.Events;

namespace Projects.Application.Commands.RemoveUserFromProject;

/// <summary>
/// Команда удаления пользователя из проекта
/// </summary>
public sealed record RemoveUserFromProjectCommand(
    Guid ProjectId,
    Guid UserId) : ICommand;

/// <summary>
/// Обработчик команды удаления пользователя из проекта
/// </summary>
internal sealed class RemoveUserFromProjectCommandHandler : ICommandHandler<RemoveUserFromProjectCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectUserRepository _projectUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public RemoveUserFromProjectCommandHandler(
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

    public async Task<Result> Handle(RemoveUserFromProjectCommand command, CancellationToken cancellationToken)
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

        var events = new List<IDomainEvent>
        {
            new ProjectUserRemovedEvent(
                projectUser.Id,
                projectUser.ProjectId,
                project.TenantId,
                projectUser.UserId,
                DateTime.UtcNow)
        };

        await _projectUserRepository.DeleteAsync(projectUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

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

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

