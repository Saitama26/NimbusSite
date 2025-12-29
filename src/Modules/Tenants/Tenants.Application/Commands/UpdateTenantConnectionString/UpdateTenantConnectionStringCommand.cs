using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Tenants.Application.Abstractions;
using Tenants.Contracts.Events;
using Tenants.Domain.Errors;

namespace Tenants.Application.Commands.UpdateTenantConnectionString;

/// <summary>
/// Команда обновления строки подключения тенанта
/// </summary>
public sealed record UpdateTenantConnectionStringCommand(
    int TenantInt,
    string ConnectionString) : ICommand<UpdateTenantConnectionStringResponse>;

/// <summary>
/// Ответ при обновлении connection string тенанта
/// </summary>
public sealed record UpdateTenantConnectionStringResponse(
    int TenantInt,
    string ConnectionString);

/// <summary>
/// Обработчик команды обновления строки подключения
/// </summary>
internal sealed class UpdateTenantConnectionStringCommandHandler : ICommandHandler<UpdateTenantConnectionStringCommand, UpdateTenantConnectionStringResponse>
{
    private readonly ITenantsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UpdateTenantConnectionStringCommandHandler> _logger;

    public UpdateTenantConnectionStringCommandHandler(
        ITenantsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        IServiceProvider serviceProvider,
        ILogger<UpdateTenantConnectionStringCommandHandler> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Result<UpdateTenantConnectionStringResponse>> Handle(UpdateTenantConnectionStringCommand command, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.TenantInt == command.TenantInt, cancellationToken);

        if (tenant == null)
        {
            return Result<UpdateTenantConnectionStringResponse>.Failure(TenantErrors.NotFound(command.TenantInt));
        }

        // Обновление ConnectionString
        // Извлекаем базовое имя БД (без суффикса _число)
        var baseDatabaseName = ExtractDatabaseName(command.ConnectionString);
        
        if (string.IsNullOrEmpty(baseDatabaseName))
        {
            return Result<UpdateTenantConnectionStringResponse>.Failure(
                Error.Validation("Tenant.InvalidConnectionString",
                    "Database name is not specified in connection string"));
        }
        
        // Удаляем существующий суффикс _число из базового имени, если есть
        var numberSuffixPattern = @"_(\d+)$";
        var numberMatch = Regex.Match(baseDatabaseName, numberSuffixPattern);
        if (numberMatch.Success)
        {
            baseDatabaseName = baseDatabaseName.Substring(0, baseDatabaseName.Length - numberMatch.Value.Length);
        }
        
        // Используем TenantInt (id) текущего тенанта для имени БД
        // Имя БД будет в формате: baseName_{TenantInt}
        var finalDatabaseName = $"{baseDatabaseName}_{tenant.TenantInt}";
        
        _logger.LogInformation(
            "Using TenantInt {TenantInt} for database name '{DatabaseName}'",
            tenant.TenantInt, finalDatabaseName);
        
        // Обновляем connection string с финальным именем БД
        var normalizedConnectionString = ReplaceDatabaseNameInConnectionString(command.ConnectionString, finalDatabaseName);
        
        if (tenant.ConnectionString != normalizedConnectionString)
        {
            tenant.ConnectionString = normalizedConnectionString;
            tenant.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Инициализируем БД тенанта с новой connection string (создаем БД если не существует)
            try
            {
                var databaseInitializerType = Type.GetType("Common.Infrastructure.Tenancy.TenantDatabaseInitializer, Common.Infrastructure");
                if (databaseInitializerType != null)
                {
                    // Получаем TenantDatabaseInitializer через reflection
                    var tenantsConnectionString = Environment.GetEnvironmentVariable("TENANTS_DB_CONNECTION_STRING")
                        ?? throw new InvalidOperationException("TENANTS_DB_CONNECTION_STRING is not configured");
                    
                    var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger(databaseInitializerType);
                    
                    var databaseInitializer = Activator.CreateInstance(
                        databaseInitializerType,
                        tenantsConnectionString,
                        logger);
                    
                    if (databaseInitializer != null)
                    {
                        // Вызываем InitializeTenantDatabaseAsync через reflection
                        // Передаем уже нормализованную connection string (с _{TenantInt} в имени БД)
                        // InitializeTenantDatabaseAsync проверит, что имя БД заканчивается на _{TenantInt} и оставит как есть
                        var method = databaseInitializerType.GetMethod("InitializeTenantDatabaseAsync");
                        if (method != null)
                        {
                            var task = method.Invoke(databaseInitializer, new object[] { tenant.TenantInt, normalizedConnectionString, cancellationToken }) as Task;
                            if (task != null)
                            {
                                await task;
                                _logger.LogInformation("Database initialized for tenant {TenantInt} with updated connection string", tenant.TenantInt);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CRITICAL: Failed to initialize database for tenant {TenantInt} with updated connection string. " +
                                   "Error: {Error}", tenant.TenantInt, ex.Message);
                // Пробрасываем исключение - обновление connection string без создания БД может привести к ошибкам
                return Result<UpdateTenantConnectionStringResponse>.Failure(
                    Error.Failure("Tenant.DatabaseInitializationFailed",
                        $"Failed to initialize database for tenant {tenant.TenantInt}. Error: {ex.Message}"));
            }

            // Обновляем кэш connection strings СИНХРОННО
            // Используем reflection для получения TenantConnectionCache без прямой зависимости
            try
            {
                var connectionCacheType = Type.GetType("Common.Infrastructure.Tenancy.TenantConnectionCache, Common.Infrastructure");
                if (connectionCacheType != null)
                {
                    var connectionCache = _serviceProvider.GetService(connectionCacheType);
                    if (connectionCache != null)
                    {
                        var setConnectionStringMethod = connectionCacheType.GetMethod("SetConnectionString");
                        if (setConnectionStringMethod != null)
                        {
                            // Используем нормализованную connection string (с _{TenantInt} в имени БД)
                            setConnectionStringMethod.Invoke(connectionCache, new object[] { tenant.TenantInt, normalizedConnectionString });
                            _logger.LogInformation("Connection string updated in cache for tenant {TenantInt}", tenant.TenantInt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update connection string in cache for tenant {TenantInt}", tenant.TenantInt);
                // Не пробрасываем исключение, событие все равно будет обработано через Kafka
            }

            // Публикация интеграционного события в Kafka для других модулей
            // Используем нормализованную connection string (с _{TenantInt} в имени БД)
            var @event = new TenantConnectionStringUpdatedEvent(
                tenant.TenantInt,
                normalizedConnectionString,
                tenant.UpdatedAt);

            await _eventBus.PublishAsync(@event, cancellationToken);

            return Result<UpdateTenantConnectionStringResponse>.Success(
                new UpdateTenantConnectionStringResponse(tenant.TenantInt, normalizedConnectionString));
        }

        // Connection string не изменился, но возвращаем ответ с TenantInt
        return Result<UpdateTenantConnectionStringResponse>.Success(
            new UpdateTenantConnectionStringResponse(tenant.TenantInt, tenant.ConnectionString));
    }

    /// <summary>
    /// Обеспечить, чтобы имя БД в connection string заканчивалось на _{tenantInt}
    /// Если имя БД уже содержит _{tenantInt} в конце, оставляет как есть
    /// Иначе добавляет _{tenantInt} к имени БД
    /// </summary>
    private string EnsureDatabaseNameHasTenantId(string connectionString, int tenantInt)
    {
        // Парсим connection string вручную, чтобы не добавлять зависимость от MySqlConnector
        // Ищем параметр Database= или database=
        var databasePattern = @"(?i)(?:^|;)\s*(?:Database|database)\s*=\s*([^;]+)";
        var match = Regex.Match(connectionString, databasePattern);
        
        if (!match.Success || match.Groups.Count < 2)
        {
            return connectionString; // Если имя БД не найдено, возвращаем как есть (валидация будет позже)
        }

        var databaseName = match.Groups[1].Value.Trim();
        
        if (string.IsNullOrEmpty(databaseName))
        {
            return connectionString; // Если имя БД пустое, возвращаем как есть
        }

        // Всегда добавляем _{tenantInt} из базы данных в конец имени БД
        // Удаляем любой суффикс _число в конце (если есть) и добавляем _{tenantInt} из БД
        // Это гарантирует, что имя БД всегда будет соответствовать реальному id тенанта из БД
        string newDatabaseName;
        var numberSuffixPattern = @"_(\d+)$";
        var numberMatch = Regex.Match(databaseName, numberSuffixPattern);
        
        if (numberMatch.Success && numberMatch.Groups.Count >= 2)
        {
            // Имя БД заканчивается на _число, удаляем это число и добавляем _{tenantInt} из базы данных
            var baseName = databaseName.Substring(0, databaseName.Length - numberMatch.Value.Length);
            newDatabaseName = $"{baseName}_{tenantInt}";
        }
        else
        {
            // Имя БД не заканчивается на _число, добавляем _{tenantInt} из базы данных
            newDatabaseName = $"{databaseName}_{tenantInt}";
        }
        
        // Заменяем старое значение Database на новое
        var replacement = Regex.Replace(
            connectionString,
            databasePattern,
            m => m.Value.Replace(databaseName, newDatabaseName),
            RegexOptions.IgnoreCase);
        
        return replacement;
    }

    /// <summary>
    /// Извлечь имя БД из connection string
    /// </summary>
    private string? ExtractDatabaseName(string connectionString)
    {
        var databasePattern = @"(?i)(?:^|;)\s*(?:Database|database)\s*=\s*([^;]+)";
        var match = Regex.Match(connectionString, databasePattern);
        
        if (match.Success && match.Groups.Count >= 2)
        {
            return match.Groups[1].Value.Trim();
        }
        
        return null;
    }


    /// <summary>
    /// Заменить имя БД в connection string
    /// </summary>
    private string ReplaceDatabaseNameInConnectionString(string connectionString, string newDatabaseName)
    {
        var databasePattern = @"(?i)(?:^|;)\s*((?:Database|database)\s*=\s*)([^;]+)";
        var replacement = Regex.Replace(
            connectionString,
            databasePattern,
            m => $"{m.Groups[1].Value}{newDatabaseName}",
            RegexOptions.IgnoreCase);
        
        return replacement;
    }
}

