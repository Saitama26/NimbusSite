using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Tenants.Application.Abstractions;
using Tenants.Application.Commands.CreateTenant;
using Tenants.Domain.Entities;
using Tenants.Domain.Enums;
using Tenants.Domain.Errors;
using Tenants.Domain.Events;

namespace Tenants.Application.Commands.CreateTenant;

/// <summary>
/// Команда создания нового тенанта
/// </summary>
public sealed record CreateTenantCommand(
    string Name,
    string Subdomain,
    string? ConnectionString = null,
    string? Description = null,
    string? AdminEmail = null) : ICommand<CreateTenantResponse>;



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
        // Проверка уникальности поддомена
        var subdomainLower = command.Subdomain.ToLowerInvariant().Trim();
        var exists = await _repository.ExistsBySubdomainAsync(subdomainLower, cancellationToken);
        if (exists)
        {
            return Result<CreateTenantResponse>.Failure(TenantErrors.SubdomainAlreadyExists(subdomainLower));
        }

        // Создание тенанта
        var tenant = new Tenant
        {
            Name = command.Name.Trim(),
            Subdomain = subdomainLower,
            ConnectionString = command.ConnectionString,
            Description = command.Description?.Trim(),
            AdminEmail = command.AdminEmail?.Trim(),
            Status = TenantStatus.Active,
        };

        var events = new List<IDomainEvent>
        {
            new TenantCreatedEvent(
                tenant.Id,
                tenant.Name,
                tenant.Subdomain,
                tenant.Status,
                tenant.Description,
                tenant.AdminEmail,
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

