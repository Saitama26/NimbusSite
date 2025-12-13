using Common.Application.Abstractions.Events;
using Identity.Application.Abstractions;
using Identity.Domain.Enums;
using Microsoft.Extensions.Logging;
using Contracts.Users.Events;

namespace Identity.Application.EventHandlers;

/// <summary>
/// Обработчик события удаления пользователя
/// Отзывает все активные сессии и токены
/// </summary>
internal sealed class UserDeletedEventHandler : IEventHandler<UserDeletedEvent>
{
    private readonly IUserCredentialsRepository _credentialsRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserDeletedEventHandler> _logger;

    public UserDeletedEventHandler(
        IUserCredentialsRepository credentialsRepository,
        ISessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserDeletedEventHandler> logger)
    {
        _credentialsRepository = credentialsRepository;
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UserDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserDeletedEvent for UserId: {UserId}",
            domainEvent.UserId);

        // Отзываем все активные сессии
        var sessions = await _sessionRepository.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
        var activeSessions = sessions.Where(s => s.IsActive).ToList();
        
        foreach (var session in activeSessions)
        {
            session.Status = SessionStatus.Revoked;
            session.ClosedAt = DateTime.UtcNow;
            session.CloseReason = "User deleted";
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }

        // Отзываем все refresh токены
        var tokens = await _refreshTokenRepository.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
        var validTokens = tokens.Where(t => t.IsValid).ToList();

        foreach (var token in validTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevocationReason = "User deleted";
            await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
        }

        // Блокируем учетные данные (не удаляем, чтобы сохранить историю)
        var credentials = await _credentialsRepository.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
        if (credentials != null)
        {
            credentials.LockedOutUntil = DateTime.UtcNow.AddYears(100); // Фактическая блокировка
            credentials.UpdatedAt = DateTime.UtcNow;
            await _credentialsRepository.UpdateAsync(credentials, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User credentials locked and all sessions/tokens revoked for UserId: {UserId}",
            domainEvent.UserId);
    }
}

