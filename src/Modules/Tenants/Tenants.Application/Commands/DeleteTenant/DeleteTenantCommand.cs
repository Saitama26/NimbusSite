using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Tenants.Application.Abstractions;
using Tenants.Contracts.Events;
using Tenants.Contracts.Enums;
using Tenants.Domain.Enums;
using Tenants.Domain.Errors;

namespace Tenants.Application.Commands.DeleteTenant;

/// <summary>
/// Команда удаления тенанта (soft delete)
/// </summary>
public sealed record DeleteTenantCommand(int TenantInt) : ICommand;

/// <summary>
/// Обработчик команды удаления тенанта (soft delete)
/// </summary>
internal sealed class DeleteTenantCommandHandler : ICommandHandler<DeleteTenantCommand>
{
    private readonly ITenantsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public DeleteTenantCommandHandler(
        ITenantsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteTenantCommand command, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(t => t.TenantInt == command.TenantInt, cancellationToken);

        if (tenant == null)
        {
            return Result.Failure(TenantErrors.NotFound(command.TenantInt));
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционных событий
        var statusChangedEvent = new TenantStatusChangedEvent(
            tenant.TenantInt,
            (TenantStatusContract)(int)oldStatus,
            TenantStatusContract.Deleted,
            tenant.UpdatedAt);

        var deletedEvent = new TenantDeletedEvent(
            tenant.TenantInt,
            tenant.UpdatedAt);

        await _eventBus.PublishAsync(statusChangedEvent, cancellationToken);
        await _eventBus.PublishAsync(deletedEvent, cancellationToken);

        return Result.Success();
    }
}

