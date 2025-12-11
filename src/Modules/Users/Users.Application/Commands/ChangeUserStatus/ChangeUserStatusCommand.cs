using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Users.Application.Abstractions;
using Users.Domain.Enums;
using Users.Domain.Errors;
using Users.Domain.Events;

namespace Users.Application.Commands.ChangeUserStatus;

/// <summary>
/// Команда изменения статуса пользователя
/// </summary>
public sealed record ChangeUserStatusCommand(
    Guid UserId,
    UserStatus NewStatus) : ICommand;

/// <summary>
/// Обработчик команды изменения статуса пользователя
/// </summary>
internal sealed class ChangeUserStatusCommandHandler : ICommandHandler<ChangeUserStatusCommand>
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangeUserStatusCommandHandler(
        IUserRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangeUserStatusCommand command, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(command.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        if (user.Status == command.NewStatus)
        {
            return Result.Success(); // Статус уже установлен
        }

        var oldStatus = user.Status;
        user.Status = command.NewStatus;
        user.UpdatedAt = DateTime.UtcNow;

        var events = new List<IDomainEvent>
        {
            new UserStatusChangedEvent(user.Id, user.TenantId, oldStatus, command.NewStatus, user.UpdatedAt)
        };

        await _repository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Валидатор команды изменения статуса пользователя
/// </summary>
internal sealed class ChangeUserStatusCommandValidator : AbstractValidator<ChangeUserStatusCommand>
{
    public ChangeUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid user status.");
    }
}

