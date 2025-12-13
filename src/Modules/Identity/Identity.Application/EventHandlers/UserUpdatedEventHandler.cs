using Common.Application.Abstractions.Events;
using Identity.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Contracts.Users.Events;

namespace Identity.Application.EventHandlers;

/// <summary>
/// Обработчик события обновления пользователя
/// Синхронизирует email в учетных данных, если он изменился
/// </summary>
internal sealed class UserUpdatedEventHandler : IEventHandler<UserUpdatedEvent>
{
    private readonly IUserCredentialsRepository _credentialsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserUpdatedEventHandler> _logger;

    public UserUpdatedEventHandler(
        IUserCredentialsRepository credentialsRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserUpdatedEventHandler> logger)
    {
        _credentialsRepository = credentialsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(UserUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserUpdatedEvent for UserId: {UserId}",
            domainEvent.UserId);

        var credentials = await _credentialsRepository.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
        if (credentials == null)
        {
            _logger.LogWarning(
                "UserCredentials not found for UserId: {UserId}. Skipping update.",
                domainEvent.UserId);
            return;
        }

        // Обновляем UpdatedAt
        credentials.UpdatedAt = DateTime.UtcNow;

        await _credentialsRepository.UpdateAsync(credentials, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UserCredentials updated for UserId: {UserId}",
            domainEvent.UserId);
    }
}

