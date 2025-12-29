using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Users.Contracts.Events;

namespace Identity.Infrastructure.EventHandlers;

/// <summary>
/// Обработчик события создания пользователя
/// Создает учетные данные для нового пользователя
/// </summary>
internal sealed class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly EventHandlersTenantDbContextFactory _dbContextFactory;
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(
        EventHandlersTenantDbContextFactory dbContextFactory,
        ILogger<UserCreatedEventHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserCreatedEvent for UserId: {UserId}, Email: {Email}, TenantId: {TenantId}",
            domainEvent.UserId,
            domainEvent.Email,
            domainEvent.TenantId);

        // Создаем DbContext для тенанта из события
        using var dbContext = await _dbContextFactory.CreateDbContextForTenantAsync<IdentityDbContext>(
            domainEvent.TenantId,
            options => new IdentityDbContext(options),
            cancellationToken);

        // Проверяем, не созданы ли уже учетные данные
        var existing = await dbContext.UserCredentials
            .FirstOrDefaultAsync(c => c.UserId == domainEvent.UserId, cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning(
                "UserCredentials already exists for UserId: {UserId}. Skipping creation.",
                domainEvent.UserId);
            return;
        }

        // Создаем учетные данные с пустым паролем
        // Пароль должен быть установлен отдельно (например, через команду SetPassword или при регистрации)
        // TenantId берется из события
        var credentials = new UserCredentials
        {
            UserId = domainEvent.UserId,
            TenantId = domainEvent.TenantId,
            Email = domainEvent.Email.ToLowerInvariant().Trim(),
            PasswordHash = string.Empty, // Пароль будет установлен позже
            EmailConfirmed = false,
            FailedLoginAttempts = 0
        };

        dbContext.UserCredentials.Add(credentials);
        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UserCredentials created for UserId: {UserId}, Email: {Email}, TenantId: {TenantId}",
            domainEvent.UserId,
            domainEvent.Email,
            domainEvent.TenantId);
    }
}
