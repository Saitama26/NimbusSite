using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using Contracts.Identity.Events;

namespace Identity.Application.Commands.Logout;

/// <summary>
/// Команда выхода пользователя из системы
/// </summary>
public sealed record LogoutCommand(
    Guid SessionId,
    string? Reason = null) : ICommand;

/// <summary>
/// Обработчик команды выхода
/// </summary>
internal sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public LogoutCommandHandler(
        ISessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(command.SessionId, cancellationToken);
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

        // Отзываем связанный refresh token, если есть
        if (session.RefreshTokenId.HasValue)
        {
            var refreshToken = await _refreshTokenRepository.GetByIdAsync(session.RefreshTokenId.Value, cancellationToken);
            if (refreshToken != null && refreshToken.IsValid)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevocationReason = "User logged out";
                await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
            }
        }

        var events = new List<IDomainEvent>
        {
            new UserLoggedOutEvent(
                session.UserId,
                session.TenantId,
                session.Id,
                DateTime.UtcNow,
                command.Reason)
        };

        await _sessionRepository.UpdateAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

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
    }
}

