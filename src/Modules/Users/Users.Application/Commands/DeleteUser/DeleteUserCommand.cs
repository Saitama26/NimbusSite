using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Users.Application.Abstractions;
using Users.Domain.Enums;
using Contracts.Users;
using Users.Domain.Errors;
using Contracts.Users.Events;

namespace Users.Application.Commands.DeleteUser;

/// <summary>
/// Команда удаления пользователя (soft delete)
/// </summary>
public sealed record DeleteUserCommand(Guid UserId) : ICommand;

/// <summary>
/// Обработчик команды удаления пользователя
/// </summary>
internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public DeleteUserCommandHandler(
        IUserRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
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

        var oldStatus = user.Status;
        user.Status = UserStatus.Deleted;
        user.UpdatedAt = DateTime.UtcNow;

        var events = new List<IDomainEvent>
        {
            new UserStatusChangedEvent(user.Id, (UserStatusContract)(int)oldStatus, UserStatusContract.Deleted, user.UpdatedAt),
            new UserDeletedEvent(user.Id, user.UpdatedAt)
        };

        await _repository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Валидатор команды удаления пользователя
/// </summary>
internal sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

