using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Identity.Domain.Entities;
using Users.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.EventHandlers;

/// <summary>
/// Обработчик события регистрации пользователя
/// Обновляет UserCredentials с хешем пароля
/// </summary>
internal sealed class UserRegisteredEventHandler : IEventHandler<UserRegisteredEvent>
{
    private readonly EventHandlersTenantDbContextFactory _dbContextFactory;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(
        EventHandlersTenantDbContextFactory dbContextFactory,
        ILogger<UserRegisteredEventHandler> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserRegisteredEvent for UserId: {UserId}, Email: {Email}, TenantId: {TenantId}",
            domainEvent.UserId,
            domainEvent.Email,
            domainEvent.TenantId);

        // Создаем DbContext для тенанта из события
        using var dbContext = await _dbContextFactory.CreateDbContextForTenantAsync<IdentityDbContext>(
            domainEvent.TenantId,
            options => new IdentityDbContext(options),
            cancellationToken);

        // Получаем или создаем учетные данные
        var credentials = await dbContext.UserCredentials
            .FirstOrDefaultAsync(c => c.UserId == domainEvent.UserId, cancellationToken);
        
        if (credentials == null)
        {
            // Если UserCredentials еще не созданы (не должно быть, но на всякий случай)
            _logger.LogWarning(
                "UserCredentials not found for UserId: {UserId}. Creating new credentials.",
                domainEvent.UserId);

            credentials = new UserCredentials
            {
                UserId = domainEvent.UserId,
                TenantId = domainEvent.TenantId,
                Email = domainEvent.Email.ToLowerInvariant().Trim(),
                PasswordHash = domainEvent.PasswordHash,
                EmailConfirmed = false,
                FailedLoginAttempts = 0
            };

            dbContext.UserCredentials.Add(credentials);
        }
        else
        {
            // Обновляем существующие учетные данные с хешем пароля
            credentials.PasswordHash = domainEvent.PasswordHash;
            credentials.PasswordChangedAt = DateTime.UtcNow;
            credentials.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UserCredentials updated with password hash for UserId: {UserId}, Email: {Email}, TenantId: {TenantId}",
            domainEvent.UserId,
            domainEvent.Email,
            domainEvent.TenantId);
    }
}

