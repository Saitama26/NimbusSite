using System.Data;
using Common.Application.Abstractions.Tenancy;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Common.Infrastructure.Tenancy;

/// <summary>
/// Реализация ITenancyDomain, читающая метаданные тенанта из центральной БД NimbusSite_Tenants.
/// Используется модулями (Users, Projects, Tasks, Identity, AccessPermissions), чтобы получать
/// TenantInt и connection string без прямой зависимости от Tenants.Infrastructure.
/// </summary>
internal sealed class TenancyDomainService : ITenancyDomain
{
    private readonly string _connectionString;
    private readonly ILogger<TenancyDomainService> _logger;

    public TenancyDomainService(string connectionString, ILogger<TenancyDomainService> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger;
    }

    public async Task<int> FindTenantIntAsync(string tenantName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantName))
            throw new ArgumentException("Tenant name cannot be empty", nameof(tenantName));

        const string sql = @"SELECT TenantInt FROM Tenants.Tenants WHERE Name = @name LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@name", tenantName);

        var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (result == null || result == DBNull.Value)
            throw new InvalidOperationException($"Tenant '{tenantName}' not found");

        return Convert.ToInt32(result);
    }

    public async Task<TenantInfo> GetTenantInfoAsync(int tenantInt, CancellationToken cancellationToken = default)
    {
        if (tenantInt <= 0)
            throw new ArgumentException("TenantInt must be greater than zero", nameof(tenantInt));

        const string sql = @"SELECT TenantInt, Name, ConnectionString, CreatedAt 
                             FROM Tenants.Tenants 
                             WHERE TenantInt = @tenantInt
                             LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@tenantInt", tenantInt);

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException($"Tenant with TenantInt '{tenantInt}' not found");

        var connectionString = reader.GetString(reader.GetOrdinal("ConnectionString"));
        return new TenantInfo
        {
            TenantInt = tenantInt,
            Name = reader.GetString(reader.GetOrdinal("Name")),
            ConnectionString = connectionString,
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };
    }
}

