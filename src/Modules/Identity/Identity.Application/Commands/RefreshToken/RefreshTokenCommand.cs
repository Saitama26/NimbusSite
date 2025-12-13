using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Identity.Domain.Errors;
using Contracts.Identity.Events;
using IdentityUnitOfWork = Identity.Application.Abstractions.IUnitOfWork;

namespace Identity.Application.Commands.RefreshToken;

/// <summary>
/// Команда обновления токена
/// </summary>
public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<RefreshTokenResponse>;

/// <summary>
/// Ответ при обновлении токена
/// </summary>
public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    DateTime RefreshTokenExpiresAt);

/// <summary>
/// Обработчик команды обновления токена
/// </summary>
internal sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ITokenHasher _tokenHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IdentityUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ISessionRepository sessionRepository,
        ITokenHasher tokenHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IdentityUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _sessionRepository = sessionRepository;
        _tokenHasher = tokenHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        // 1. Вычислить хеш refresh token
        var tokenHash = _tokenHasher.HashToken(command.RefreshToken);

        // 2. Найти токен по хешу
        var oldRefreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (oldRefreshToken == null)
        {
            return Result<RefreshTokenResponse>.Failure(IdentityErrors.RefreshTokenNotFound);
        }

        // 3. Проверить валидность токена (не истек, не отозван)
        if (!oldRefreshToken.IsValid)
        {
            if (oldRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return Result<RefreshTokenResponse>.Failure(IdentityErrors.RefreshTokenExpired);
            }
            if (oldRefreshToken.RevokedAt != null)
            {
                return Result<RefreshTokenResponse>.Failure(IdentityErrors.RefreshTokenRevoked);
            }
        }

        // 4. Найти связанную сессию
        var session = await _sessionRepository.GetByRefreshTokenIdAsync(oldRefreshToken.Id, cancellationToken);
        if (session == null || !session.IsActive)
        {
            return Result<RefreshTokenResponse>.Failure(IdentityErrors.SessionNotFound(oldRefreshToken.Id));
        }

        // 5. Отозвать старый refresh token
        oldRefreshToken.RevokedAt = DateTime.UtcNow;
        oldRefreshToken.RevocationReason = "Token refreshed";
        await _refreshTokenRepository.UpdateAsync(oldRefreshToken, cancellationToken);

        // 6. Создать новый refresh token
        var newRefreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenHasher.HashToken(newRefreshTokenValue);
        var refreshTokenExpiration = _jwtTokenGenerator.GetRefreshTokenExpiration();

        var newRefreshToken = new Identity.Domain.Entities.RefreshToken
        {
            UserId = oldRefreshToken.UserId,
            TenantId = oldRefreshToken.TenantId,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = refreshTokenExpiration,
            IpAddress = command.IpAddress,
            UserAgent = command.UserAgent
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        // 7. Обновить сессию
        session.RefreshTokenId = newRefreshToken.Id;
        session.LastActivityAt = DateTime.UtcNow;
        session.ExpiresAt = refreshTokenExpiration;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        // 8. Сгенерировать новый access token
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(oldRefreshToken.UserId, oldRefreshToken.TenantId);
        var accessTokenExpiration = _jwtTokenGenerator.GetAccessTokenExpiration();

        // 9. Сохранить изменения
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 10. Опубликовать событие TokenRefreshedEvent
        var events = new List<IDomainEvent>
        {
            new TokenRefreshedEvent(
                oldRefreshToken.UserId,
                oldRefreshToken.TenantId,
                oldRefreshToken.Id,
                newRefreshToken.Id,
                refreshTokenExpiration,
                DateTime.UtcNow,
                command.IpAddress,
                command.UserAgent)
        };

        await _eventBus.PublishAsync(events, cancellationToken);

        // 11. Вернуть результат
        var expiresIn = (int)(accessTokenExpiration - DateTime.UtcNow).TotalSeconds;
        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(
            accessToken,
            newRefreshTokenValue,
            expiresIn,
            refreshTokenExpiration));
    }
}

/// <summary>
/// Валидатор команды обновления токена
/// </summary>
internal sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

