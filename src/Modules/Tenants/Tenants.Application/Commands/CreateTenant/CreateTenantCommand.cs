using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tenants.Application.Abstractions;
using Tenants.Contracts.Events;
using Tenants.Contracts.Enums;
using Tenants.Domain.Entities;
using Tenants.Domain.Enums;

namespace Tenants.Application.Commands.CreateTenant;

/// <summary>
/// Команда создания нового тенанта
/// Тенант - это независимая сущность, связь пользователя с тенантом создается отдельно
/// ConnectionString генерируется автоматически на основе TenantInt после сохранения
/// </summary>
public sealed record CreateTenantCommand(
    string Name,
    string? Description = null) : ICommand<CreateTenantResponse>;

/// <summary>
/// Ответ при создании тенанта
/// </summary>
public sealed record CreateTenantResponse(
    int TenantInt,
    string ConnectionString);

/// <summary>
/// Обработчик команды создания тенанта
/// </summary>
internal sealed class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, CreateTenantResponse>
{
    private readonly ITenantsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(
        ITenantsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        IServiceProvider serviceProvider,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Result<CreateTenantResponse>> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        // Проверяем, что тенант с таким именем не существует
        var exists = await _dbContext.Tenants
            .AnyAsync(t => t.Name == command.Name.Trim(), cancellationToken);

        if (exists)
        {
            return Result<CreateTenantResponse>.Failure(
                Error.Conflict("Tenant.AlreadyExists", $"Tenant with name '{command.Name}' already exists"));
        }

        // Создание тенанта
        var tenant = new Tenant
        {
            Name = command.Name.Trim(),
            ConnectionString = string.Empty, // Будет сгенерирован автоматически после сохранения
            Description = command.Description?.Trim(),
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Tenants.Add(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Генерируем connection string автоматически на основе TenantInt
        // Имя БД всегда в формате {dbName}_{TenantInt} (по умолчанию Tenant_{TenantInt})
        // Используем connection string центральной БД как шаблон
        var tenantsConnectionString = Environment.GetEnvironmentVariable("TENANTS_DB_CONNECTION_STRING")
            ?? throw new InvalidOperationException("TENANTS_DB_CONNECTION_STRING is not configured");

        // Создаем connection string с именем БД в формате Tenant_{TenantInt}
        // (уже соответствует формату {dbName}_{id})
        var tenantConnectionString = tenantsConnectionString
            .Replace("Database=NimbusSite_Tenants", $"Database=Tenant_{tenant.TenantInt}")
            .Replace("database=NimbusSite_Tenants", $"database=Tenant_{tenant.TenantInt}");

        tenant.ConnectionString = tenantConnectionString;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Создаем событие для обработки
        var @event = new TenantCreatedEvent(
            tenant.TenantInt,
            tenant.Name,
            (TenantStatusContract)(int)tenant.Status,
            tenant.Description,
            tenant.ConnectionString,
            tenant.CreatedAt);

        // Инициализируем БД тенанта СИНХРОННО сразу после создания
        // Это гарантирует, что БД будет создана сразу, без ожидания Kafka
        // Используем обработчик события для единообразия логики
        try
        {
            // Вызываем обработчик события локально для немедленной инициализации БД
            using var scope = _serviceProvider.CreateScope();
            var eventHandler = scope.ServiceProvider.GetRequiredService<IEventHandler<TenantCreatedEvent>>();
            await eventHandler.Handle(@event, cancellationToken);
            _logger.LogInformation("Tenant database initialized synchronously for tenant {TenantInt}", tenant.TenantInt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRITICAL: Failed to initialize tenant database synchronously for tenant {TenantInt}. " +
                                "Database tables were NOT created! This is a critical error.", tenant.TenantInt);
            // Пробрасываем исключение - создание тенанта без БД не имеет смысла
            // Это критическая ошибка, которая должна быть видна пользователю
            return Result<CreateTenantResponse>.Failure(
                Error.Failure("Tenant.DatabaseInitializationFailed", 
                    $"Failed to initialize database for tenant. Error: {ex.Message}"));
        }

        // Публикация интеграционного события в Kafka для других модулей
        await _eventBus.PublishAsync(@event, cancellationToken);

        return Result<CreateTenantResponse>.Success(
            new CreateTenantResponse(tenant.TenantInt, tenant.ConnectionString));
    }
}

