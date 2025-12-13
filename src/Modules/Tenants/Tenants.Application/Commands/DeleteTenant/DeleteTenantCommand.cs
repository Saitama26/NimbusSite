using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Tenants.Application.Abstractions;
using Tenants.Domain.Enums;
using Contracts.Tenants;
using Tenants.Domain.Errors;
using Contracts.Tenants.Events;

namespace Tenants.Application.Commands.DeleteTenant;

/// <summary>
/// Команда удаления тенанта (soft delete)
/// </summary>
public sealed record DeleteTenantCommand(Guid TenantId) : ICommand;

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
        var events = new List<IDomainEvent>
        {
            new TenantStatusChangedEvent(tenant.Id, (TenantStatusContract)(int)oldStatus, TenantStatusContract.Deleted, tenant.UpdatedAt),
            new TenantDeletedEvent(tenant.Id, tenant.UpdatedAt)
        };

        // Сохранение
        await _repository.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация событий
        await _eventBus.PublishAsync(events, cancellationToken);

        return Result.Success();
    }
}

