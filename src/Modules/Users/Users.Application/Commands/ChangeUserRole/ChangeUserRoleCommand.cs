using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Users.Application.Abstractions;
using Users.Domain.Enums;
using Users.Domain.Errors;
using Users.Domain.Events;

namespace Users.Application.Commands.ChangeUserRole;

/// <summary>
/// Команда изменения роли пользователя
/// </summary>
public sealed record ChangeUserRoleCommand(
    Guid UserId,
    UserRole NewRole) : ICommand;

/// <summary>
/// Обработчик команды изменения роли пользователя
/// </summary>
internal sealed class ChangeUserRoleCommandHandler : ICommandHandler<ChangeUserRoleCommand>
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangeUserRoleCommandHandler(
        IUserRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangeUserRoleCommand command, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(command.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        if (user.Status == UserStatus.Deleted)
        {
            return Result.Failure(UserErrors.AlreadyDeleted);
        }

        if (user.Role == command.NewRole)
        {
            return Result.Success(); // Роль уже установлена
        }

        var oldRole = user.Role;
        user.Role = command.NewRole;
        user.UpdatedAt = DateTime.UtcNow;

        var events = new List<IDomainEvent>
        {
            new UserRoleChangedEvent(user.Id, user.TenantId, oldRole, command.NewRole, user.UpdatedAt)
        };

        await _repository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Валидатор команды изменения роли пользователя
/// </summary>
internal sealed class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.NewRole)
            .IsInEnum().WithMessage("Invalid user role.");
    }
}

