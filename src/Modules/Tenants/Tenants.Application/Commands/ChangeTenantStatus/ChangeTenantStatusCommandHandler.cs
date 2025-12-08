using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Tenants.Application.Abstractions;
using Tenants.Application.Commands.ChangeTenantStatus;
using Tenants.Domain.Errors;
using Tenants.Domain.Events;

namespace Tenants.Application.Commands.ChangeTenantStatus;

/// <summary>
/// Обработчик команды изменения статуса тенанта
/// </summary>
internal sealed class ChangeTenantStatusCommandHandler : ICommandHandler<ChangeTenantStatusCommand>
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public ChangeTenantStatusCommandHandler(
        ITenantRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ChangeTenantStatusCommand command, CancellationToken cancellationToken)
    {
        // Найти тенанта
        var tenant = await _repository.GetByIdAsync(command.TenantId, cancellationToken);
        if (tenant == null)
        {
            return Result.Failure(TenantErrors.NotFound(command.TenantId));
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
        tenant.AddDomainEvent(new TenantStatusChangedEvent(tenant.Id, oldStatus, command.NewStatus, tenant.UpdatedAt.Value));

        // Сохранение
        await _repository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация событий
        if (tenant.DomainEvents.Any())
        {
            await _eventBus.PublishAsync(tenant.DomainEvents, cancellationToken);
            tenant.ClearDomainEvents();
        }

        return Result.Success();
    }
}

