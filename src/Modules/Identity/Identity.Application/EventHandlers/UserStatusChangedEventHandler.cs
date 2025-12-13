using Common.Application.Abstractions.Events;
using Identity.Application.Abstractions;
using Identity.Domain.Enums;
using Microsoft.Extensions.Logging;
using Users.Domain.Enums;
using Contracts.Users;
using Contracts.Users.Events;

namespace Identity.Application.EventHandlers;

/// <summary>
/// Обработчик события изменения статуса пользователя
/// Блокирует/разблокирует доступ в зависимости от статуса
/// </summary>
internal sealed class UserStatusChangedEventHandler : IEventHandler<UserStatusChangedEvent>
{
    private readonly IUserCredentialsRepository _credentialsRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserStatusChangedEventHandler> _logger;

    public UserStatusChangedEventHandler(
        IUserCredentialsRepository credentialsRepository,
        ISessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserStatusChangedEventHandler> logger)
    {
        _credentialsRepository = credentialsRepository;
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UserStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserStatusChangedEvent for UserId: {UserId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
            domainEvent.UserId,
            domainEvent.OldStatus,
            domainEvent.NewStatus);

        var credentials = await _credentialsRepository.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
        if (credentials == null)
        {
            _logger.LogWarning(
                "UserCredentials not found for UserId: {UserId}. Skipping status change.",
                domainEvent.UserId);
            return;
        }

        // Если статус стал Inactive или Deleted - блокируем доступ
        if (domainEvent.NewStatus == UserStatusContract.Inactive || domainEvent.NewStatus == UserStatusContract.Deleted)
        {
            credentials.LockedOutUntil = DateTime.UtcNow.AddYears(100); // Фактическая блокировка
            credentials.UpdatedAt = DateTime.UtcNow;

            // Отзываем все активные сессии
            var sessions = await _sessionRepository.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
            var activeSessions = sessions.Where(s => s.IsActive).ToList();

            foreach (var session in activeSessions)
            {
                session.Status = SessionStatus.Revoked;
                session.ClosedAt = DateTime.UtcNow;
                session.CloseReason = $"User status changed to {domainEvent.NewStatus}";
                await _sessionRepository.UpdateAsync(session, cancellationToken);
            }

            // Отзываем все refresh токены
            var tokens = await _refreshTokenRepository.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
            var validTokens = tokens.Where(t => t.IsValid).ToList();

            foreach (var token in validTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevocationReason = $"User status changed to {domainEvent.NewStatus}";
                await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
            }
        }
        // Если статус стал Active - разблокируем доступ
        else if (domainEvent.NewStatus == UserStatusContract.Active && domainEvent.OldStatus != UserStatusContract.Active)
        {
            credentials.LockedOutUntil = null;
            credentials.UpdatedAt = DateTime.UtcNow;
        }

        await _credentialsRepository.UpdateAsync(credentials, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User credentials updated for UserId: {UserId}, NewStatus: {NewStatus}",
            domainEvent.UserId,
            domainEvent.NewStatus);
    }
}

