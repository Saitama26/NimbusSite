using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using Identity.Application.Abstractions;
using Identity.Contracts.Events;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Commands.ChangePassword;

/// <summary>
/// Команда изменения пароля пользователя
/// </summary>
public sealed record ChangePasswordCommand(
    Guid UserId,
    int TenantId,
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
    private readonly IIdentityDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangePasswordCommandHandler(
        IIdentityDbContext dbContext,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        // 1. Найти учетные данные пользователя
        var credentials = await _dbContext.UserCredentials
            .FirstOrDefaultAsync(c => c.UserId == command.UserId && c.TenantId == command.TenantId, cancellationToken);
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

        // 5. Если RevokeAllSessions = true, отозвать все сессии и токены
        if (command.RevokeAllSessions)
        {
            // Отозвать все активные сессии
            var sessions = await _dbContext.Sessions
                .Where(s => s.UserId == command.UserId && s.TenantId == command.TenantId && s.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var session in sessions)
            {
                session.Status = SessionStatus.Revoked;
                session.ClosedAt = DateTime.UtcNow;
                session.CloseReason = "Password changed";

                // Отозвать связанный refresh token, если есть
                if (session.RefreshTokenId.HasValue)
                {
                    var refreshToken = await _dbContext.RefreshTokens
                        .FirstOrDefaultAsync(rt => rt.Id == session.RefreshTokenId.Value, cancellationToken);
                    if (refreshToken != null && refreshToken.IsValid)
                    {
                        refreshToken.RevokedAt = DateTime.UtcNow;
                        refreshToken.RevocationReason = "Password changed";
                    }
                }
            }
        }

        // 6. Сохранить изменения
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Опубликовать событие PasswordChangedEvent
        var @event = new PasswordChangedEvent(
            command.UserId,
            credentials.TenantId,
            command.RevokeAllSessions,
            DateTime.UtcNow,
            command.IpAddress,
            command.UserAgent);

        await _eventBus.PublishAsync(@event, cancellationToken);

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

        RuleFor(x => x.TenantId)
            .GreaterThan(0).WithMessage("Tenant ID must be greater than 0.");

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

