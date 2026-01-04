using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions;
using Users.Contracts.Events;
using Users.Contracts.Enums;
using Users.Domain.Enums;
using Users.Domain.Errors;

namespace Users.Application.Commands.DeleteUser;

/// <summary>
/// Команда удаления пользователя (soft delete)
/// </summary>
public sealed record DeleteUserCommand(Guid UserId, int TenantId) : ICommand;

/// <summary>
/// Обработчик команды удаления пользователя
/// </summary>
internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUsersDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public DeleteUserCommandHandler(
        IUsersDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId && u.TenantId == command.TenantId, cancellationToken);
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционных событий
        var statusChangedEvent = new UserStatusChangedEvent(
            user.Id,
            user.TenantId,
            (UserStatusContract)(int)oldStatus,
            UserStatusContract.Deleted,
            user.UpdatedAt);

        var deletedEvent = new UserDeletedEvent(
            user.Id,
            user.TenantId,
            user.UpdatedAt);

        await _eventBus.PublishAsync(statusChangedEvent, cancellationToken);
        await _eventBus.PublishAsync(deletedEvent, cancellationToken);

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

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID must be greater than 0.");
    }
}

