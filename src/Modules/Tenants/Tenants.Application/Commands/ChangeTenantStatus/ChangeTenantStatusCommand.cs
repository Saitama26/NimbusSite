using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tenants.Application.Abstractions;
using Tenants.Contracts.Events;
using Tenants.Contracts.Enums;
using Tenants.Domain.Enums;
using Tenants.Domain.Errors;

namespace Tenants.Application.Commands.ChangeTenantStatus;

/// <summary>
/// Команда изменения статуса тенанта
/// </summary>
public sealed record ChangeTenantStatusCommand(
    int TenantInt,
    TenantStatus NewStatus) : ICommand;

/// <summary>
/// Обработчик команды изменения статуса тенанта
/// </summary>
internal sealed class ChangeTenantStatusCommandHandler : ICommandHandler<ChangeTenantStatusCommand>
{
    private readonly ITenantsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ChangeTenantStatusCommandHandler> _logger;

    public ChangeTenantStatusCommandHandler(
        ITenantsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        ILogger<ChangeTenantStatusCommandHandler> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result> Handle(ChangeTenantStatusCommand command, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.TenantInt == command.TenantInt, cancellationToken);

        if (tenant == null)
        {
            return Result.Failure(TenantErrors.NotFound(command.TenantInt));
        }

        // Проверка, что статус изменился
        if (tenant.Status == command.NewStatus)
        {
            return Result.Success();
        }

        var oldStatus = tenant.Status;

        // Изменение статуса
        tenant.Status = command.NewStatus;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события в Kafka для других модулей
        var @event = new TenantStatusChangedEvent(
            tenant.TenantInt,
            (TenantStatusContract)(int)oldStatus,
            (TenantStatusContract)(int)command.NewStatus,
            tenant.UpdatedAt);

        await _eventBus.PublishAsync(@event, cancellationToken);

        _logger.LogInformation("Tenant {TenantInt} status changed: {OldStatus} -> {NewStatus}", 
            tenant.TenantInt, oldStatus, command.NewStatus);

        return Result.Success();
    }
}

