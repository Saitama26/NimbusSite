using AccessPermissions.Application.Abstractions;
using AccessPermissions.Domain.Entities;
using Common.Application.Abstractions.Events;
using Contracts.Users.Events;
using Microsoft.Extensions.Logging;

namespace AccessPermissions.Application.EventHandlers;

/// <summary>
/// Удаляет права доступа для удаленного пользователя.
/// </summary>
internal sealed class UserDeletedEventHandler : IEventHandler<UserDeletedEvent>
{
    private readonly IAccessPermissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserDeletedEventHandler> _logger;

    public UserDeletedEventHandler(
        IAccessPermissionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UserDeletedEventHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UserDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteByUserIdAsync(domainEvent.UserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Removed {Count} permissions for deleted UserId {UserId}", deleted, domainEvent.UserId);
    }
}

