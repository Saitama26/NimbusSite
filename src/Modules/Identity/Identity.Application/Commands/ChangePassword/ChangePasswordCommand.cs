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
using IdentityUnitOfWork = Identity.Application.Abstractions.IUnitOfWork;

namespace Identity.Application.Commands.ChangePassword;

/// <summary>
/// Команда изменения пароля пользователя
/// </summary>
public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword,
    bool RevokeAllSessions = true,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand;

/// <summary>
/// Обработчик команды изменения пароля
/// </summary>
internal sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IUserCredentialsRepository _credentialsRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IdentityUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangePasswordCommandHandler(
        IUserCredentialsRepository credentialsRepository,
        ISessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IdentityUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _credentialsRepository = credentialsRepository;
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        // 1. Найти учетные данные пользователя
        var credentials = await _credentialsRepository.GetByUserIdAsync(command.UserId, cancellationToken);
        if (credentials == null)
        {
            return Result.Failure(IdentityErrors.UserNotFound(command.UserId));
        }

        // 2. Проверить текущий пароль
        if (string.IsNullOrWhiteSpace(credentials.PasswordHash) ||
            !_passwordHasher.VerifyPassword(command.CurrentPassword, credentials.PasswordHash))
        {
            return Result.Failure(IdentityErrors.PasswordMismatch);
        }

        // 3. Хешировать новый пароль
        var newPasswordHash = _passwordHasher.HashPassword(command.NewPassword);

        // 4. Обновить пароль пользователя
        credentials.PasswordHash = newPasswordHash;
        credentials.PasswordChangedAt = DateTime.UtcNow;
        credentials.UpdatedAt = DateTime.UtcNow;
        await _credentialsRepository.UpdateAsync(credentials, cancellationToken);

        // 5. Если RevokeAllSessions = true, отозвать все сессии и токены
        if (command.RevokeAllSessions)
        {
            // Отозвать все активные сессии
            var sessions = await _sessionRepository.GetByUserIdAsync(command.UserId, cancellationToken);
            var activeSessions = sessions.Where(s => s.IsActive).ToList();

            foreach (var session in activeSessions)
            {
                session.Status = SessionStatus.Revoked;
                session.ClosedAt = DateTime.UtcNow;
                session.CloseReason = "Password changed";
                await _sessionRepository.UpdateAsync(session, cancellationToken);

                // Отозвать связанный refresh token, если есть
                if (session.RefreshTokenId.HasValue)
                {
                    var refreshToken = await _refreshTokenRepository.GetByIdAsync(session.RefreshTokenId.Value, cancellationToken);
                    if (refreshToken != null && refreshToken.IsValid)
                    {
                        refreshToken.RevokedAt = DateTime.UtcNow;
                        refreshToken.RevocationReason = "Password changed";
                        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
                    }
                }
            }
        }

        // 6. Сохранить изменения
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Опубликовать событие PasswordChangedEvent
        var tenantId = credentials.TenantId ?? Guid.Empty; // Если TenantId null, используем Guid.Empty
        var events = new List<IDomainEvent>
        {
            new PasswordChangedEvent(
                command.UserId,
                tenantId,
                command.RevokeAllSessions,
                DateTime.UtcNow,
                command.IpAddress,
                command.UserAgent)
        };

        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Валидатор команды изменения пароля
/// </summary>
internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long.")
            .MaximumLength(100).WithMessage("New password must not exceed 100 characters.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("New password must contain at least one uppercase letter, one lowercase letter, and one digit.");

        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password.");
    }
}

