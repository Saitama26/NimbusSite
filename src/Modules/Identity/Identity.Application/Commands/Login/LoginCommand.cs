using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using Contracts.Identity.Events;
using Users.Application.Abstractions;
using Users.Domain.Enums;
using IdentityUnitOfWork = Identity.Application.Abstractions.IUnitOfWork;

namespace Identity.Application.Commands.Login;

/// <summary>
/// Команда входа пользователя в систему
/// </summary>
public sealed record LoginCommand(
    Guid TenantId,
    string Email,
    string Password,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<LoginResponse>;

/// <summary>
/// Ответ при входе пользователя
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    Guid SessionId,
    int ExpiresIn,
    DateTime RefreshTokenExpiresAt);

/// <summary>
/// Обработчик команды входа
/// </summary>
internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserCredentialsRepository _credentialsRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly IdentityUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUserCredentialsRepository credentialsRepository,
        ISessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ITokenHasher tokenHasher,
        IdentityUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _userRepository = userRepository;
        _credentialsRepository = credentialsRepository;
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        // 1. Найти пользователя по email
        var emailLower = command.Email.ToLowerInvariant().Trim();
        var user = await _userRepository.GetByEmailAsync(emailLower, cancellationToken);
        if (user == null)
        {
            return Result<LoginResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        // 2. Проверить статус пользователя
        if (user.Status != UserStatus.Active)
        {
            return Result<LoginResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        // 3. Найти учетные данные (глобально по email, так как email уникальный)
        var credentials = await _credentialsRepository.GetByEmailGlobalAsync(emailLower, cancellationToken);
        if (credentials == null)
        {
            return Result<LoginResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        // 3.1. Проверить, что пользователь в нужном тенанте (если TenantId указан)
        if (credentials.TenantId.HasValue && credentials.TenantId.Value != command.TenantId)
        {
            return Result<LoginResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        // 4. Проверить блокировку аккаунта
        if (credentials.IsLockedOut)
        {
            return Result<LoginResponse>.Failure(IdentityErrors.UserLockedOut);
        }

        // 5. Проверить пароль
        if (string.IsNullOrWhiteSpace(credentials.PasswordHash) || 
            !_passwordHasher.VerifyPassword(command.Password, credentials.PasswordHash))
        {
            // Увеличиваем счетчик неудачных попыток
            credentials.FailedLoginAttempts++;
            if (credentials.FailedLoginAttempts >= 5)
            {
                credentials.LockedOutUntil = DateTime.UtcNow.AddMinutes(30); // Блокировка на 30 минут
            }
            credentials.UpdatedAt = DateTime.UtcNow;
            await _credentialsRepository.UpdateAsync(credentials, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Result<LoginResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        // 6. Проверить подтверждение email (опционально, можно пропустить на этапе разработки)
        // if (!credentials.EmailConfirmed)
        // {
        //     return Result<LoginResponse>.Failure(IdentityErrors.EmailNotConfirmed);
        // }

        // 7. Сбросить счетчик неудачных попыток при успешном входе
        credentials.FailedLoginAttempts = 0;
        credentials.LockedOutUntil = null;
        credentials.UpdatedAt = DateTime.UtcNow;
        await _credentialsRepository.UpdateAsync(credentials, cancellationToken);

        // 8. Сгенерировать токены
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user.Id, command.TenantId);
        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = _tokenHasher.HashToken(refreshTokenValue);
        var accessTokenExpiration = _jwtTokenGenerator.GetAccessTokenExpiration();
        var refreshTokenExpiration = _jwtTokenGenerator.GetRefreshTokenExpiration();

        // 9. Создать refresh token
        var refreshToken = new Identity.Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TenantId = command.TenantId,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshTokenExpiration,
            IpAddress = command.IpAddress,
            UserAgent = command.UserAgent
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        // 10. Создать сессию
        var session = new Session
        {
            UserId = user.Id,
            TenantId = command.TenantId,
            RefreshTokenId = refreshToken.Id,
            Status = SessionStatus.Active,
            IpAddress = command.IpAddress,
            UserAgent = command.UserAgent,
            LastActivityAt = DateTime.UtcNow,
            ExpiresAt = refreshTokenExpiration
        };

        await _sessionRepository.AddAsync(session, cancellationToken);

        // 11. Сохранить все изменения
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 12. Опубликовать событие UserLoggedInEvent
        var events = new List<IDomainEvent>
        {
            new UserLoggedInEvent(
                user.Id,
                command.TenantId,
                session.Id,
                DateTime.UtcNow,
                command.IpAddress,
                command.UserAgent)
        };

        await _eventBus.PublishAsync(events, cancellationToken);

        // 13. Вернуть результат
        var expiresIn = (int)(accessTokenExpiration - DateTime.UtcNow).TotalSeconds;
        return Result<LoginResponse>.Success(new LoginResponse(
            accessToken,
            refreshTokenValue,
            session.Id,
            expiresIn,
            refreshTokenExpiration));
    }
}

/// <summary>
/// Валидатор команды входа
/// </summary>
internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
    }
}

