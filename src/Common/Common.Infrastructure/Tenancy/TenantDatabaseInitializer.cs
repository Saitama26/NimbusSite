using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Common.Infrastructure.Tenancy;

/// <summary>
/// Сервис для инициализации базы данных тенанта
/// Создает БД с именем из connection string (по умолчанию Tenant_{id})
/// Миграции применяются каждым модулем автоматически при первом обращении к БД (lazy initialization)
/// </summary>
public sealed class TenantDatabaseInitializer
{
    private readonly string _tenantsDbConnectionString;
    private readonly ILogger<TenantDatabaseInitializer> _logger;

    public TenantDatabaseInitializer(
        string tenantsDbConnectionString,
        ILogger<TenantDatabaseInitializer> logger)
    {
        _tenantsDbConnectionString = tenantsDbConnectionString;
        _logger = logger;
    }

    /// <summary>
    /// Инициализировать базу данных для тенанта
    /// Упрощенная версия: только создает БД, миграции применяются каждым модулем при первом обращении
    /// Имя БД всегда заканчивается на _{tenantInt} в формате {dbName}_{tenantInt}
    /// </summary>
    /// <param name="tenantInt">Числовой идентификатор тенанта</param>
    /// <param name="customConnectionString">Кастомная connection string (опционально). Если не указана, генерируется автоматически</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <exception cref="InvalidOperationException">Выбрасывается, если connection string некорректна (не указано имя БД в параметре Database)</exception>
    public async Task InitializeTenantDatabaseAsync(
        int tenantInt,
        string? customConnectionString = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== STARTING DATABASE INITIALIZATION FOR TENANT {TenantInt} ===", tenantInt);

        // Получаем connection string для БД тенанта с добавлением _{tenantInt} к имени БД
        var tenantConnectionString = customConnectionString != null
            ? EnsureDatabaseNameHasTenantId(customConnectionString, tenantInt)
            : BuildDefaultConnectionString(tenantInt);

        // Извлекаем имя БД из connection string
        var builder = new MySqlConnectionStringBuilder(tenantConnectionString);
        var databaseName = builder.Database;

        // Валидация: имя БД должно быть указано в connection string
        if (string.IsNullOrEmpty(databaseName))
        {
            var errorMessage = $"Connection string for tenant {tenantInt} is invalid: database name is not specified. " +
                             "Connection string must include Database parameter (e.g., Database=Tenant_2 or Database=MyCustomDB).";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        // Создаем БД если не существует
        await CreateDatabaseIfNotExistsAsync(databaseName, tenantConnectionString, cancellationToken);

        _logger.LogInformation("Database initialization completed for tenant {TenantInt}. " +
                              "Database: {DatabaseName}. " +
                              "Migrations will be applied automatically by each module on first access.",
                              tenantInt, databaseName);

        _logger.LogInformation("=== DATABASE INITIALIZATION COMPLETED FOR TENANT {TenantInt} ===", tenantInt);
    }

    /// <summary>
    /// Обеспечить, чтобы имя БД в connection string заканчивалось на _{tenantInt} из базы данных
    /// Всегда удаляет любой суффикс _число в конце имени БД и добавляет _{tenantInt} из БД
    /// Это гарантирует соответствие между реальным id тенанта из БД и именем БД
    /// </summary>
    /// <param name="connectionString">Исходная connection string</param>
    /// <param name="tenantInt">Идентификатор тенанта из базы данных</param>
    /// <returns>Connection string с обновленным именем БД</returns>
    public string EnsureDatabaseNameHasTenantId(string connectionString, int tenantInt)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        if (string.IsNullOrEmpty(databaseName))
        {
            return connectionString; // Если имя БД не указано, возвращаем как есть (валидация будет позже)
        }

        // Всегда добавляем _{tenantInt} из базы данных в конец имени БД
        // Удаляем любой суффикс _число в конце (если есть) и добавляем _{tenantInt} из БД
        // Это гарантирует, что имя БД всегда будет соответствовать реальному id тенанта из БД
        string newDatabaseName;
        var numberSuffixPattern = @"_(\d+)$";
        var numberMatch = System.Text.RegularExpressions.Regex.Match(databaseName, numberSuffixPattern);
        
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
        
        builder.Database = newDatabaseName;
        
        return builder.ConnectionString;
    }

    /// <summary>
    /// Создать БД если не существует
    /// </summary>
    private async Task CreateDatabaseIfNotExistsAsync(
        string databaseName,
        string tenantConnectionString,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating database {DatabaseName} if not exists", databaseName);

        // Используем connection string тенанта, но без указания Database для подключения к серверу
        var builder = new MySqlConnectionStringBuilder(tenantConnectionString);
        var serverConnectionString = builder.ConnectionString; // Сохраняем оригинальный
        builder.Database = null; // Подключаемся без указания БД для создания

        await using var connection = new MySqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var createDbSql = $@"
            CREATE DATABASE IF NOT EXISTS `{databaseName}` 
            CHARACTER SET utf8mb4 
            COLLATE utf8mb4_unicode_ci;";

        await using var command = new MySqlCommand(createDbSql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("Database {DatabaseName} created or already exists", databaseName);
    }

    /// <summary>
    /// Построить connection string по умолчанию для тенанта
    /// </summary>
    private string BuildDefaultConnectionString(int tenantInt)
    {
        // Используем connection string центральной БД как шаблон
        var builder = new MySqlConnectionStringBuilder(_tenantsDbConnectionString);
        builder.Database = $"Tenant_{tenantInt}";
        return builder.ConnectionString;
    }
}
