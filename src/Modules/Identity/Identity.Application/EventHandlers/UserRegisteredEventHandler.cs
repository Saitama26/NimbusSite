using Common.Application.Abstractions.Events;
using Common.Domain.Events;
using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Microsoft.Extensions.Logging;
using Contracts.Users.Events;

namespace Identity.Application.EventHandlers;

/// <summary>
/// Обработчик события регистрации пользователя
/// Обновляет UserCredentials с хешем пароля
/// </summary>
internal sealed class UserRegisteredEventHandler : IEventHandler<UserRegisteredEvent>
{
    private readonly IUserCredentialsRepository _credentialsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(
        IUserCredentialsRepository credentialsRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserRegisteredEventHandler> logger)
    {
        _credentialsRepository = credentialsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserRegisteredEvent for UserId: {UserId}, Email: {Email}",
            domainEvent.UserId,
            domainEvent.Email);

        // Получаем или создаем учетные данные
        var credentials = await _credentialsRepository.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
        
        if (credentials == null)
        {
            // Если UserCredentials еще не созданы (не должно быть, но на всякий случай)
            _logger.LogWarning(
                "UserCredentials not found for UserId: {UserId}. Creating new credentials.",
                domainEvent.UserId);

            credentials = new UserCredentials
            {
                UserId = domainEvent.UserId,
                TenantId = null, // Пользователь может быть без тенанта
                Email = domainEvent.Email.ToLowerInvariant().Trim(),
                PasswordHash = domainEvent.PasswordHash,
                EmailConfirmed = false,
                FailedLoginAttempts = 0
            };

            await _credentialsRepository.AddAsync(credentials, cancellationToken);
        }
        else
        {
            // Обновляем существующие учетные данные с хешем пароля
            credentials.PasswordHash = domainEvent.PasswordHash;
            credentials.PasswordChangedAt = DateTime.UtcNow;
            credentials.UpdatedAt = DateTime.UtcNow;

            await _credentialsRepository.UpdateAsync(credentials, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UserCredentials updated with password hash for UserId: {UserId}, Email: {Email}",
            domainEvent.UserId,
            domainEvent.Email);
    }
}

