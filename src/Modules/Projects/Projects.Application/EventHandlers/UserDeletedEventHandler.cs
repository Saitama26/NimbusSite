using Common.Application.Abstractions.Events;
using Contracts.Users.Events;
using Projects.Application.Abstractions;
using Projects.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Projects.Application.EventHandlers;

/// <summary>
/// Удаляет пользователя из всех проектов при его удалении.
/// </summary>
internal sealed class UserDeletedEventHandler : IEventHandler<UserDeletedEvent>
{
    private readonly IProjectUserRepository _projectUserRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserDeletedEventHandler> _logger;

    public UserDeletedEventHandler(
        IProjectUserRepository projectUserRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserDeletedEventHandler> logger)
    {
        _projectUserRepository = projectUserRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UserDeletedEvent domainEvent, CancellationToken cancellationToken)
    {
        var memberships = await _projectUserRepository.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
        var list = memberships.ToList();
        if (list.Count == 0)
        {
            return;
        }

        foreach (var m in list)
        {
            await _projectUserRepository.DeleteAsync(m, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Removed {Count} project memberships for deleted UserId {UserId}", list.Count, domainEvent.UserId);
    }
}

