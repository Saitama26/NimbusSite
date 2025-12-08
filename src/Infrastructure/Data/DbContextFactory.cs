using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

internal sealed class DbContextFactory : IDbContextFactory
{
    private readonly IShardConnectionProvider _shardConnectionProvider;
    private readonly ITenantContext _tenantContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbContextFactory> _logger;

    public DbContextFactory(
        IShardConnectionProvider shardConnectionProvider,
        ITenantContext tenantContext,
        IServiceProvider serviceProvider,
        ILogger<DbContextFactory> logger)
    {
        _shardConnectionProvider = shardConnectionProvider;
        _tenantContext = tenantContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<IApplicationDbContext> CreateDbContextAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var connectionString = await _shardConnectionProvider.GetConnectionStringAsync(tenantId, cancellationToken);

        // Используем ServerVersion для MySQL 8.0
        // Для Pomelo 9.0 используется ServerVersion.Parse для указания версии MySQL
        var serverVersion = ServerVersion.Parse("8.0.21");
        
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, serverVersion);

        // Получаем зависимости из DI контейнера
        var domainEventDispatcher = _serviceProvider.GetRequiredService<IDomainEventDispatcher>();
        var logger = _serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        _logger.LogInformation("Creating DbContext for tenant {TenantId}", tenantId);

        return new ApplicationDbContext(
            optionsBuilder.Options,
            domainEventDispatcher,
            logger);
    }

    public async Task<IApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            throw new InvalidOperationException("TenantId is not set in context");
        }

        return await CreateDbContextAsync(_tenantContext.TenantId.Value, cancellationToken);
    }
}

