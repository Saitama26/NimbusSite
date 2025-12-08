using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Tenants.Application.Abstractions;
using Tenants.Application.Commands.DeleteTenant;
using Tenants.Domain.Enums;
using Tenants.Domain.Errors;
using Tenants.Domain.Events;

namespace Tenants.Application.Commands.DeleteTenant;

/// <summary>
/// Обработчик команды удаления тенанта (soft delete)
/// </summary>
internal sealed class DeleteTenantCommandHandler : ICommandHandler<DeleteTenantCommand>
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public DeleteTenantCommandHandler(
        ITenantRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteTenantCommand command, CancellationToken cancellationToken)
    {
        // Найти тенанта
        var tenant = await _repository.GetByIdAsync(command.TenantId, cancellationToken);
        if (tenant == null)
        {
            return Result.Failure(TenantErrors.NotFound(command.TenantId));
        }

        // Проверка, что тенант еще не удален
        if (tenant.Status == TenantStatus.Deleted)
        {
            return Result.Failure(TenantErrors.AlreadyDeleted);
        }

        var oldStatus = tenant.Status;

        // Soft delete - изменение статуса на Deleted
        tenant.Status = TenantStatus.Deleted;
        tenant.UpdatedAt = DateTime.UtcNow;
        tenant.AddDomainEvent(new TenantStatusChangedEvent(tenant.Id, oldStatus, TenantStatus.Deleted, tenant.UpdatedAt.Value));
        tenant.AddDomainEvent(new TenantDeletedEvent(tenant.Id, tenant.UpdatedAt.Value));

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

