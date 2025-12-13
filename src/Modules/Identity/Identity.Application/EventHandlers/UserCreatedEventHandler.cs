using Common.Application.Abstractions.Events;
using Common.Domain.Events;
using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Microsoft.Extensions.Logging;
using Contracts.Users.Events;

namespace Identity.Application.EventHandlers;

/// <summary>
/// Обработчик события создания пользователя
/// Создает учетные данные для нового пользователя
/// </summary>
internal sealed class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IUserCredentialsRepository _credentialsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(
        IUserCredentialsRepository credentialsRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserCreatedEventHandler> logger)
    {
        _credentialsRepository = credentialsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserCreatedEvent for UserId: {UserId}, Email: {Email}",
            domainEvent.UserId,
            domainEvent.Email);

        // Проверяем, не созданы ли уже учетные данные
        var existing = await _credentialsRepository.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning(
                "UserCredentials already exists for UserId: {UserId}. Skipping creation.",
                domainEvent.UserId);
            return;
        }

        // Создаем учетные данные с пустым паролем
        // Пароль должен быть установлен отдельно (например, через команду SetPassword или при регистрации)
        // TenantId не устанавливается, так как пользователь может быть без тенанта
        var credentials = new UserCredentials
        {
            UserId = domainEvent.UserId,
            TenantId = null, // Пользователь может быть без тенанта
            Email = domainEvent.Email.ToLowerInvariant().Trim(),
            PasswordHash = string.Empty, // Пароль будет установлен позже
            EmailConfirmed = false,
            FailedLoginAttempts = 0
        };

        await _credentialsRepository.AddAsync(credentials, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UserCredentials created for UserId: {UserId}, Email: {Email}",
            domainEvent.UserId,
            domainEvent.Email);
    }
}
