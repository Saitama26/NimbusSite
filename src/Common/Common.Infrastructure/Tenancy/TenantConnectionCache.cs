using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Collections.Concurrent;

namespace Common.Infrastructure.Tenancy;

/// <summary>
/// Кэш connection strings тенантов
/// Загружает все connection strings при старте и хранит в памяти
/// </summary>
public sealed class TenantConnectionCache
{
    private readonly ConcurrentDictionary<int, string> _connectionStrings = new();
    private readonly string _tenantsDbConnectionString;
    private readonly ILogger<TenantConnectionCache> _logger;
    private bool _isLoaded;

    public TenantConnectionCache(string tenantsDbConnectionString, ILogger<TenantConnectionCache> logger)
    {
        _tenantsDbConnectionString = tenantsDbConnectionString;
        _logger = logger;
    }

    /// <summary>
    /// Загрузить все connection strings из БД тенантов
    /// Вызывать при старте приложения
    /// </summary>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Loading tenant connection strings from database...");

        const string sql = "SELECT TenantInt, ConnectionString FROM Tenants WHERE ConnectionString IS NOT NULL";

        await using var connection = new MySqlConnection(_tenantsDbConnectionString);
        await connection.OpenAsync(cancellationToken);
        
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var count = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            var tenantInt = reader.GetInt32(0);
            var connectionString = reader.GetString(1);
            _connectionStrings[tenantInt] = connectionString;
            count++;
        }

        _isLoaded = true;
        _logger.LogInformation("Loaded {Count} tenant connection strings", count);
    }

    /// <summary>
    /// Получить connection string для тенанта (синхронно из кэша или БД)
    /// </summary>
    public string GetConnectionString(int tenantInt)
    {
        // Сначала пробуем получить из кэша
        if (_connectionStrings.TryGetValue(tenantInt, out var connectionString))
        {
            return connectionString;
        }

        // Если не найдено в кэше, пробуем загрузить из БД (fallback)
        // Это нужно на случай, если событие еще не дошло до этого модуля через Kafka
        _logger.LogDebug("Connection string for tenant {TenantInt} not found in cache, loading from database", tenantInt);
        
        try
        {
            var dbConnectionString = LoadConnectionStringFromDatabase(tenantInt);
            if (!string.IsNullOrEmpty(dbConnectionString))
            {
                // Добавляем в кэш для последующих запросов
                _connectionStrings[tenantInt] = dbConnectionString;
                _logger.LogInformation("Loaded connection string for tenant {TenantInt} from database and added to cache", tenantInt);
                return dbConnectionString;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load connection string for tenant {TenantInt} from database", tenantInt);
        }

        throw new InvalidOperationException($"Connection string for tenant {tenantInt} not found in cache or database.");
    }

    /// <summary>
    /// Загрузить connection string для конкретного тенанта из БД (синхронно)
    /// </summary>
    private string? LoadConnectionStringFromDatabase(int tenantInt)
    {
        const string sql = "SELECT ConnectionString FROM Tenants WHERE TenantInt = @tenantInt AND ConnectionString IS NOT NULL";
        
        using var connection = new MySqlConnection(_tenantsDbConnectionString);
        connection.Open();
        
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@tenantInt", tenantInt);
        
        var result = command.ExecuteScalar();
        return result?.ToString();
    }

    /// <summary>
    /// Проверить, есть ли тенант в кэше
    /// </summary>
    public bool HasTenant(int tenantInt) => _connectionStrings.ContainsKey(tenantInt);

    /// <summary>
    /// Добавить/обновить connection string (для динамического добавления тенантов)
    /// </summary>
    public void SetConnectionString(int tenantInt, string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("Attempted to set empty connection string for tenant {TenantInt}", tenantInt);
            return;
        }

        _connectionStrings[tenantInt] = connectionString;
        
        // Если кэш еще не загружен, помечаем как частично загруженный
        // Это позволяет использовать SetConnectionString до полной загрузки кэша
        if (!_isLoaded)
        {
            _logger.LogDebug("Connection string set for tenant {TenantInt} before cache is fully loaded", tenantInt);
        }
        else
        {
            _logger.LogInformation("Added/updated connection string for tenant {TenantInt} in cache", tenantInt);
        }
    }

    /// <summary>
    /// Удалить connection string тенанта из кэша
    /// </summary>
    public void RemoveConnectionString(int tenantInt)
    {
        if (_connectionStrings.TryRemove(tenantInt, out _))
        {
            _logger.LogInformation("Removed connection string for tenant {TenantInt}", tenantInt);
        }
    }

    /// <summary>
    /// Перезагрузить кэш
    /// </summary>
    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        _connectionStrings.Clear();
        await LoadAsync(cancellationToken);
    }
}

