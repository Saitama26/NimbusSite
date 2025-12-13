using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Users.Application.Abstractions;
using Users.Domain.Enums;
using Users.Domain.Errors;
using Contracts.Users.Events;

namespace Users.Application.Commands.UpdateUser;

/// <summary>
/// Команда обновления пользователя
/// </summary>
public sealed record UpdateUserCommand(
    Guid UserId,
    string? Name = null,
    string? Phone = null,
    string? Bio = null) : ICommand;

/// <summary>
/// Обработчик команды обновления пользователя
/// </summary>
internal sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public UpdateUserCommandHandler(
        IUserRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
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

        // Обновление полей, если они указаны
        var hasChanges = false;
        if (!string.IsNullOrWhiteSpace(command.Name))
        {
            var name = command.Name.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure(UserErrors.NameEmpty);
            }
            user.Name = name;
            hasChanges = true;
        }

        if (command.Phone != null)
        {
            user.Phone = string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim();
            hasChanges = true;
        }

        if (command.Bio != null)
        {
            user.Bio = string.IsNullOrWhiteSpace(command.Bio) ? null : command.Bio.Trim();
            hasChanges = true;
        }

        if (!hasChanges)
        {
            return Result.Success(); // Нет изменений
        }

        user.UpdatedAt = DateTime.UtcNow;

        var events = new List<IDomainEvent>
        {
            new UserUpdatedEvent(
                user.Id,
                user.Name,
                user.Phone,
                user.Bio,
                user.UpdatedAt)
        };

        await _repository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Валидатор команды обновления пользователя
/// </summary>
internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("User name cannot be empty.")
            .MaximumLength(200).WithMessage("User name must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Bio));
    }
}

