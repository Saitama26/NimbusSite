using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Contracts.Events;
using Identity.Domain.Entities;
using Identity.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.RefreshToken;

/// <summary>
/// Команда обновления токена
/// </summary>
public sealed record RefreshTokenCommand(
    string RefreshToken,
    int TenantId,
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
    private readonly IIdentityDbContext _dbContext;
    private readonly ITokenHasher _tokenHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public RefreshTokenCommandHandler(
        IIdentityDbContext dbContext,
        ITokenHasher tokenHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _tokenHasher = tokenHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        // 1. Вычислить хеш refresh token
        var tokenHash = _tokenHasher.HashToken(command.RefreshToken);

        // 2. Найти токен по хешу и TenantId
        var oldRefreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.TenantId == command.TenantId, cancellationToken);
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

        // 4. Найти связанную сессию с проверкой TenantId
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.RefreshTokenId == oldRefreshToken.Id && s.TenantId == command.TenantId, cancellationToken);
        if (session == null || !session.IsActive)
        {
            return Result<RefreshTokenResponse>.Failure(IdentityErrors.SessionNotFound(oldRefreshToken.Id));
        }

        // 5. Отозвать старый refresh token
        oldRefreshToken.RevokedAt = DateTime.UtcNow;
        oldRefreshToken.RevocationReason = "Token refreshed";

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

        _dbContext.RefreshTokens.Add(newRefreshToken);

        // 7. Обновить сессию
        session.RefreshTokenId = newRefreshToken.Id;
        session.LastActivityAt = DateTime.UtcNow;
        session.ExpiresAt = refreshTokenExpiration;

        // 8. Сохранить изменения
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 9. Сгенерировать новый access token
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(oldRefreshToken.UserId, oldRefreshToken.TenantId);
        var accessTokenExpiration = _jwtTokenGenerator.GetAccessTokenExpiration();

        // 10. Опубликовать событие TokenRefreshedEvent
        var @event = new TokenRefreshedEvent(
            oldRefreshToken.UserId,
            oldRefreshToken.TenantId,
            oldRefreshToken.Id,
            newRefreshToken.Id,
            refreshTokenExpiration,
            DateTime.UtcNow,
            command.IpAddress,
            command.UserAgent);

        await _eventBus.PublishAsync(@event, cancellationToken);

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

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID must be greater than 0.");
    }
}

