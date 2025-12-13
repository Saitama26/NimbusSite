using Common.Application.Abstractions.Events;
using Common.Domain.Events;
using Tenants.Application.Abstractions;
using Tenants.Domain.Entities;
using Microsoft.Extensions.Logging;
using Contracts.Tenants.Events;
using Users.Domain.Enums;

namespace Tenants.Application.EventHandlers;

/// <summary>
/// Обработчик события создания тенанта
/// Создает связь UserTenant с IsOwner = true для пользователя, создавшего тенант
/// </summary>
internal sealed class TenantCreatedEventHandler : IEventHandler<TenantCreatedEvent>
{
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TenantCreatedEventHandler> _logger;

    public TenantCreatedEventHandler(
        IUserTenantRepository userTenantRepository,
        IUnitOfWork unitOfWork,
        ILogger<TenantCreatedEventHandler> logger)
    {
        _userTenantRepository = userTenantRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(TenantCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling TenantCreatedEvent for TenantId: {TenantId}, CreatedByUserId: {CreatedByUserId}",
            domainEvent.TenantId,
            domainEvent.CreatedByUserId);

        // Проверяем, не создана ли уже связь UserTenant
        var existing = await _userTenantRepository.GetByUserAndTenantAsync(
            domainEvent.CreatedByUserId,
            domainEvent.TenantId,
            cancellationToken);

        if (existing != null)
        {
            _logger.LogWarning(
                "UserTenant already exists for UserId: {UserId}, TenantId: {TenantId}. Skipping creation.",
                domainEvent.CreatedByUserId,
                domainEvent.TenantId);
            return;
        }

        // Проверяем, есть ли у пользователя уже тенант, которым он владеет
        var hasOwnerTenant = await _userTenantRepository.HasOwnerTenantAsync(
            domainEvent.CreatedByUserId,
            cancellationToken);

        if (hasOwnerTenant)
        {
            _logger.LogWarning(
                "User {UserId} already has an owner tenant. Creating UserTenant without IsOwner=true.",
                domainEvent.CreatedByUserId);
        }

        // Создаем связь UserTenant
        // IsOwner = true только если у пользователя еще нет тенанта, которым он владеет
        var userTenant = new UserTenant
        {
            UserId = domainEvent.CreatedByUserId,
            TenantId = domainEvent.TenantId,
            IsOwner = !hasOwnerTenant, // Только если у пользователя еще нет тенанта-владельца
            Role = UserRole.Admin, // Пользователь, создавший тенант, становится админом
            JoinedAt = DateTime.UtcNow
        };

        await _userTenantRepository.AddAsync(userTenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UserTenant created for UserId: {UserId}, TenantId: {TenantId}, IsOwner: {IsOwner}",
            domainEvent.CreatedByUserId,
            domainEvent.TenantId,
            userTenant.IsOwner);
    }
}

