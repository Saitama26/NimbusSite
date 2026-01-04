using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Contracts.Events;
using AccessPermissions.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace AccessPermissions.Application.Commands.DeleteAccessPermission;

/// <summary>
/// Команда удаления разрешения доступа
/// </summary>
public sealed record DeleteAccessPermissionCommand(
    Guid PermissionId,
    int TenantId) : ICommand;

/// <summary>
/// Обработчик команды удаления разрешения доступа
/// </summary>
internal sealed class DeleteAccessPermissionCommandHandler : ICommandHandler<DeleteAccessPermissionCommand>
{
    private readonly IAccessPermissionsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public DeleteAccessPermissionCommandHandler(
        IAccessPermissionsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteAccessPermissionCommand command, CancellationToken cancellationToken)
    {
        var permission = await _dbContext.AccessPermissions
            .FirstOrDefaultAsync(p => p.Id == command.PermissionId && p.TenantId == command.TenantId, cancellationToken);
        if (permission == null)
        {
            return Result.Failure(AccessPermissionErrors.NotFound(command.PermissionId));
        }

        _dbContext.AccessPermissions.Remove(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Публикация интеграционного события
        var @event = new AccessPermissionDeletedEvent(
            permission.Id,
            permission.TenantId,
            permission.UserId,
            DateTime.UtcNow);

        await _eventBus.PublishAsync(@event, cancellationToken);

        return Result.Success();
    }
}

/// <summary>
/// Валидатор команды удаления разрешения доступа
/// </summary>
internal sealed class DeleteAccessPermissionCommandValidator : AbstractValidator<DeleteAccessPermissionCommand>
{
    public DeleteAccessPermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("Permission ID is required.");
    }
}

