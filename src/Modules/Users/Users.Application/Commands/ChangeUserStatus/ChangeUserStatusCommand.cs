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

namespace Users.Application.Commands.ChangeUserStatus;

/// <summary>
/// Команда изменения статуса пользователя
/// </summary>
public sealed record ChangeUserStatusCommand(
    Guid UserId,
    int TenantId,
    UserStatus NewStatus) : ICommand;

/// <summary>
/// Обработчик команды изменения статуса пользователя
/// </summary>
internal sealed class ChangeUserStatusCommandHandler : ICommandHandler<ChangeUserStatusCommand>
{
    private readonly IUsersDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangeUserStatusCommandHandler(
        IUsersDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangeUserStatusCommand command, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId && u.TenantId == command.TenantId, cancellationToken);
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new UserStatusChangedEvent(
            user.Id,
            user.TenantId,
            (UserStatusContract)(int)oldStatus,
            (UserStatusContract)(int)command.NewStatus,
            user.UpdatedAt);

        await _eventBus.PublishAsync(@event, cancellationToken);

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

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID must be greater than 0.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid user status.");
    }
}

