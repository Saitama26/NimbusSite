using AccessPermissions.Application.Abstractions;
using AccessPermissions.Domain.Entities;
using Common.Application.Abstractions.Events;
using Contracts.Tenants.Events;
using Microsoft.Extensions.Logging;

namespace AccessPermissions.Application.EventHandlers;

/// <summary>
/// Удаляет права для удалённого тенанта.
/// </summary>
internal sealed class TenantDeletedEventHandler : IEventHandler<TenantDeletedEvent>
{
    private readonly IAccessPermissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TenantDeletedEventHandler> _logger;

    public TenantDeletedEventHandler(
        IAccessPermissionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<TenantDeletedEventHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(TenantDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteByTenantIdAsync(domainEvent.TenantId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Removed {Count} permissions for deleted TenantId {TenantId}", deleted, domainEvent.TenantId);
    }
}

