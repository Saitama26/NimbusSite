using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Tenants.Application.Abstractions;
using Tenants.Domain.Entities;
using Tenants.Domain.Enums;
using Contracts.Tenants;
using Contracts.Tenants.Events;

namespace Tenants.Application.Commands.CreateTenant;

/// <summary>
/// Команда создания нового тенанта
/// Тенант создается автоматически при создании первого проекта пользователем
/// </summary>
public sealed record CreateTenantCommand(
    string Name,
    Guid CreatedByUserId,
    string? ConnectionString = null,
    string? Description = null) : ICommand<CreateTenantResponse>;



/// <summary>
/// Обработчик команды создания тенанта
/// </summary>
internal sealed class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, CreateTenantResponse>
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public CreateTenantCommandHandler(
        ITenantRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<CreateTenantResponse>> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        // Создание тенанта (без поддомена, так как он не используется)
        var tenant = new Tenant
        {
            Name = command.Name.Trim(),
            Subdomain = string.Empty, // Не используется, но оставляем для совместимости с БД
            ConnectionString = command.ConnectionString,
            Description = command.Description?.Trim(),
            AdminEmail = null, // Админ определяется через UserTenant
            Status = TenantStatus.Active,
        };

        var events = new List<IDomainEvent>
        {
            new TenantCreatedEvent(
                tenant.Id,
                tenant.Name,
                (TenantStatusContract)(int)tenant.Status,
                command.CreatedByUserId,
                tenant.Description,
                tenant.ConnectionString,
                tenant.CreatedAt)
        };

        // Сохранение
        await _repository.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация события в очередь
        await _eventBus.PublishAsync(events, cancellationToken);

        return Result<CreateTenantResponse>.Success(new CreateTenantResponse(tenant.Id));
    }
}

