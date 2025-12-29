using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Contracts.Events;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.Logout;

/// <summary>
/// Команда выхода пользователя из системы
/// </summary>
public sealed record LogoutCommand(
    Guid SessionId,
    int TenantId,
    string? Reason = null) : ICommand;

/// <summary>
/// Обработчик команды выхода
/// </summary>
internal sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly IIdentityDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public LogoutCommandHandler(
        IIdentityDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId && s.TenantId == command.TenantId, cancellationToken);
        if (session == null)
        {
            return Result.Failure(IdentityErrors.SessionNotFound(command.SessionId));
        }

        if (session.Status != SessionStatus.Active)
        {
            return Result.Success(); // Сессия уже закрыта
        }

        // Закрываем сессию
        session.Status = SessionStatus.Closed;
        session.ClosedAt = DateTime.UtcNow;
        session.CloseReason = command.Reason;

        // Отзываем связанный refresh token, если есть (с проверкой TenantId)
        if (session.RefreshTokenId.HasValue)
        {
            var refreshToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Id == session.RefreshTokenId.Value && rt.TenantId == command.TenantId, cancellationToken);
            if (refreshToken != null && refreshToken.IsValid)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevocationReason = "User logged out";
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new UserLoggedOutEvent(
            session.UserId,
            session.TenantId,
            session.Id,
            DateTime.UtcNow,
            command.Reason);

        await _eventBus.PublishAsync(@event, cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Валидатор команды выхода
/// </summary>
internal sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID must be greater than 0.");
    }
}

