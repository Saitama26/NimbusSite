using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Application.Abstractions.Views;
using Identity.Contracts.Events;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Users.Contracts.Enums;

namespace Identity.Application.Commands.Login;

/// <summary>
/// Команда входа пользователя в систему
/// </summary>
public sealed record LoginCommand(
    int TenantId,
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
    private readonly IIdentityDbContext _dbContext;
    private readonly IUserViewRepository _userViewRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public LoginCommandHandler(
        IIdentityDbContext dbContext,
        IUserViewRepository userViewRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _userViewRepository = userViewRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        // 1. Найти пользователя по email через View
        var emailLower = command.Email.ToLowerInvariant().Trim();
        var user = await _userViewRepository.GetByEmailAsync(emailLower, cancellationToken);
        if (user == null)
        {
            return Result<LoginResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        // 2. Проверить статус пользователя
        if (user.Status != Users.Contracts.Enums.UserStatusContract.Active)
        {
            return Result<LoginResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        // 3. Проверить, что пользователь в нужном тенанте
        if (user.TenantId != command.TenantId)
        {
            return Result<LoginResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        // 4. Найти учетные данные по email и TenantId
        var credentials = await _dbContext.UserCredentials
            .FirstOrDefaultAsync(c => c.Email == emailLower && c.TenantId == command.TenantId, cancellationToken);
        if (credentials == null)
        {
            return Result<LoginResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        // 5. Проверить блокировку аккаунта
        if (credentials.IsLockedOut)
        {
            return Result<LoginResponse>.Failure(IdentityErrors.UserLockedOut);
        }

        // 6. Проверить пароль
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return Result<LoginResponse>.Failure(IdentityErrors.InvalidCredentials);
        }

        // 7. Сбросить счетчик неудачных попыток при успешном входе
        credentials.FailedLoginAttempts = 0;
        credentials.LockedOutUntil = null;
        credentials.UpdatedAt = DateTime.UtcNow;

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

        _dbContext.RefreshTokens.Add(refreshToken);

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

        _dbContext.Sessions.Add(session);

        // 11. Сохранить все изменения
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 12. Вернуть результат
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
            .GreaterThan(0).WithMessage("Tenant ID must be greater than 0.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
    }
}

