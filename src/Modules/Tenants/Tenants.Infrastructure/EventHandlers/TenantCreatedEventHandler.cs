using Common.Application.Abstractions.Events;
using Common.Infrastructure.Tenancy;
using Microsoft.Extensions.Logging;
using Tenants.Contracts.Events;

namespace Tenants.Infrastructure.EventHandlers;

/// <summary>
/// Обработчик события создания тенанта
/// Инициализирует базу данных тенанта при создании и добавляет connection string в кэш
/// </summary>
internal sealed class TenantCreatedEventHandler : IEventHandler<TenantCreatedEvent>
{
    private readonly ILogger<TenantCreatedEventHandler> _logger;
    private readonly TenantDatabaseInitializer _databaseInitializer;
    private readonly TenantConnectionCache _connectionCache;

    public TenantCreatedEventHandler(
        ILogger<TenantCreatedEventHandler> logger,
        TenantDatabaseInitializer databaseInitializer,
        TenantConnectionCache connectionCache)
    {
        _logger = logger;
        _databaseInitializer = databaseInitializer;
        _connectionCache = connectionCache;
    }

    public async Task Handle(TenantCreatedEvent integrationEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Tenant created: TenantInt={TenantInt}, Name={Name}, Status={Status}",
            integrationEvent.TenantInt,
            integrationEvent.Name,
            integrationEvent.Status);

        try
        {
            // Инициализируем базу данных тенанта
            await _databaseInitializer.InitializeTenantDatabaseAsync(
                integrationEvent.TenantInt,
                integrationEvent.ConnectionString,
                cancellationToken);

            // Добавляем connection string в кэш
            if (!string.IsNullOrEmpty(integrationEvent.ConnectionString))
            {
                _connectionCache.SetConnectionString(integrationEvent.TenantInt, integrationEvent.ConnectionString);
                _logger.LogInformation(
                    "Connection string added to cache for tenant {TenantInt}",
                    integrationEvent.TenantInt);
            }

            _logger.LogInformation(
                "Database initialized successfully for tenant {TenantInt}",
                integrationEvent.TenantInt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "CRITICAL: Failed to initialize database for tenant {TenantInt}. " +
                "Database tables were NOT created! This tenant will not be functional.",
                integrationEvent.TenantInt);
            // Пробрасываем исключение, чтобы не создавать тенант без БД
            // Это критическая ошибка, которая должна быть видна
            throw;
        }
    }
}

