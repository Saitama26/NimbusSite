using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Tenants.Application.Abstractions;
using Tenants.Domain.Errors;
using Contracts.Tenants.Events;

namespace Tenants.Application.Commands.UpdateTenant;

/// <summary>
/// Команда обновления тенанта
/// </summary>
public sealed record UpdateTenantCommand(
    Guid TenantId,
    string? Name = null,
    string? Description = null,
    string? AdminEmail = null) : ICommand;

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
            var events = new List<IDomainEvent>
            {
                new TenantUpdatedEvent(
                    tenant.Id,
                    tenant.Name,
                    tenant.Description,
                    tenant.UpdatedAt)
            };

            // Сохранение
            await _repository.UpdateAsync(tenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Публикация событий
            await _eventBus.PublishAsync(events, cancellationToken);

            return Result.Success();
        }

        // Если изменений нет - просто успех без публикации
        return Result.Success();
    }
}

