using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tenants.Application.Abstractions;
using Tenants.Contracts.Events;
using Tenants.Domain.Errors;

namespace Tenants.Application.Commands.UpdateTenant;

/// <summary>
/// Команда обновления тенанта
/// </summary>
public sealed record UpdateTenantCommand(
    int TenantInt,
    string? Name = null,
    string? Description = null) : ICommand;

/// <summary>
/// Обработчик команды обновления тенанта
/// </summary>
internal sealed class UpdateTenantCommandHandler : ICommandHandler<UpdateTenantCommand>
{
    private readonly ITenantsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UpdateTenantCommandHandler> _logger;

    public UpdateTenantCommandHandler(
        ITenantsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        IServiceProvider serviceProvider,
        ILogger<UpdateTenantCommandHandler> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTenantCommand command, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.TenantInt == command.TenantInt, cancellationToken);

        if (tenant == null)
        {
            return Result.Failure(TenantErrors.NotFound(command.TenantInt));
        }

        var hasChanges = false;

        // Обновление полей
        if (!string.IsNullOrWhiteSpace(command.Name) && tenant.Name != command.Name.Trim())
        {
            tenant.Name = command.Name.Trim();
            hasChanges = true;
        }

        if (command.Description != null && tenant.Description != command.Description.Trim())
        {
            tenant.Description = command.Description.Trim();
            hasChanges = true;
        }

        if (hasChanges)
        {
            tenant.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Публикация интеграционного события в Kafka для других модулей
            var @event = new TenantUpdatedEvent(
                tenant.TenantInt,
                tenant.Name,
                tenant.Description,
                tenant.UpdatedAt);

            await _eventBus.PublishAsync(@event, cancellationToken);

            _logger.LogInformation("Tenant {TenantInt} updated: Name={Name}, Description={Description}", 
                tenant.TenantInt, tenant.Name, tenant.Description ?? "(null)");

            return Result.Success();
        }

        // Если изменений нет - просто успех без публикации
        return Result.Success();
    }
}

