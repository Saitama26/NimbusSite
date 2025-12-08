using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Tenants.Application.Abstractions;
using Tenants.Application.Commands.UpdateTenant;
using Tenants.Domain.Errors;
using Tenants.Domain.Events;

namespace Tenants.Application.Commands.UpdateTenant;

/// <summary>
/// Обработчик команды обновления тенанта
/// </summary>
internal sealed class UpdateTenantCommandHandler : ICommandHandler<UpdateTenantCommand>
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public UpdateTenantCommandHandler(
        ITenantRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateTenantCommand command, CancellationToken cancellationToken)
    {
        // Найти тенанта
        var tenant = await _repository.GetByIdAsync(command.TenantId, cancellationToken);
        if (tenant == null)
        {
            return Result.Failure(TenantErrors.NotFound(command.TenantId));
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

        if (!string.IsNullOrWhiteSpace(command.AdminEmail) && tenant.AdminEmail != command.AdminEmail.Trim())
        {
            tenant.AdminEmail = command.AdminEmail.Trim();
            hasChanges = true;
        }

        if (hasChanges)
        {
            tenant.UpdatedAt = DateTime.UtcNow;
            tenant.AddDomainEvent(new TenantUpdatedEvent(tenant.Id, tenant.Name, tenant.UpdatedAt.Value));
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

