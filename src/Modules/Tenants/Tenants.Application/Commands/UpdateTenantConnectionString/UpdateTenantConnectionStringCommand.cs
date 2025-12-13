using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using Tenants.Application.Abstractions;
using Tenants.Domain.Errors;
using Contracts.Tenants.Events;

namespace Tenants.Application.Commands.UpdateTenantConnectionString;

/// <summary>
/// Команда обновления строки подключения тенанта
/// </summary>
public sealed record UpdateTenantConnectionStringCommand(
    Guid TenantId,
    string ConnectionString) : ICommand;

/// <summary>
/// Обработчик команды обновления строки подключения
/// </summary>
internal sealed class UpdateTenantConnectionStringCommandHandler : ICommandHandler<UpdateTenantConnectionStringCommand>
{
    private readonly ITenantRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public UpdateTenantConnectionStringCommandHandler(
        ITenantRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateTenantConnectionStringCommand command, CancellationToken cancellationToken)
    {
        // Найти тенанта
        var tenant = await _repository.GetByIdAsync(command.TenantId, cancellationToken);
        if (tenant == null)
        {
            return Result.Failure(TenantErrors.NotFound(command.TenantId));
        }

        // Обновление ConnectionString
        if (tenant.ConnectionString != command.ConnectionString)
        {
            tenant.ConnectionString = command.ConnectionString;
            tenant.UpdatedAt = DateTime.UtcNow;
            var events = new List<IDomainEvent>
            {
                new TenantConnectionStringUpdatedEvent(tenant.Id, tenant.ConnectionString, tenant.UpdatedAt)
            };

            // Сохранение
            await _repository.UpdateAsync(tenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Публикация событий
            await _eventBus.PublishAsync(events, cancellationToken);

            return Result.Success();
        }

        return Result.Success();
    }
}

