using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Tenants.Application.Abstractions;
using Tenants.Application.Commands.UpdateTenantConnectionString;
using Tenants.Domain.Errors;
using Tenants.Domain.Events;

namespace Tenants.Application.Commands.UpdateTenantConnectionString;

/// <summary>
/// Обработчик команды обновления строки подключения
/// </summary>
internal sealed class UpdateTenantConnectionStringCommandHandler : ICommandHandler<UpdateTenantConnectionStringCommand>
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public UpdateTenantConnectionStringCommandHandler(
        ITenantRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateTenantConnectionStringCommand command, CancellationToken cancellationToken)
    {
        // Найти тенанта
        var tenant = await _repository.GetByIdAsync(command.TenantId, cancellationToken);
        if (tenant == null)
        {
            return Result.Failure(TenantErrors.NotFound(command.TenantId));
        }

        // Обновление ConnectionString
        if (tenant.ConnectionString != command.ConnectionString)
        {
            tenant.ConnectionString = command.ConnectionString;
            tenant.UpdatedAt = DateTime.UtcNow;
            tenant.AddDomainEvent(new TenantConnectionStringUpdatedEvent(tenant.Id, tenant.UpdatedAt.Value));
        }

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

