using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Contracts.Enums;
using AccessPermissions.Contracts.Events;
using AccessPermissions.Domain.Enums;
using AccessPermissions.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace AccessPermissions.Application.Commands.UpdateAccessPermission;

/// <summary>
/// Команда обновления разрешения доступа
/// </summary>
public sealed record UpdateAccessPermissionCommand(
    Guid PermissionId,
    int TenantId,
    PermissionType? Type = null,
    DateTime? ExpiresAt = null,
    string? Note = null) : ICommand;

/// <summary>
/// Обработчик команды обновления разрешения доступа
/// </summary>
internal sealed class UpdateAccessPermissionCommandHandler : ICommandHandler<UpdateAccessPermissionCommand>
{
    private readonly IAccessPermissionsDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public UpdateAccessPermissionCommandHandler(
        IAccessPermissionsDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateAccessPermissionCommand command, CancellationToken cancellationToken)
    {
        var permission = await _dbContext.AccessPermissions
            .FirstOrDefaultAsync(p => p.Id == command.PermissionId && p.TenantId == command.TenantId, cancellationToken);
        if (permission == null)
        {
            return Result.Failure(AccessPermissionErrors.NotFound(command.PermissionId));
        }

        // Проверка на истечение
        if (!permission.IsValid)
        {
            return Result.Failure(AccessPermissionErrors.PermissionExpired);
        }

        var hasChanges = false;

        // Обновление полей
        if (command.Type.HasValue && permission.Type != command.Type.Value)
        {
            permission.Type = command.Type.Value;
            hasChanges = true;
        }

        if (command.ExpiresAt.HasValue)
        {
            if (command.ExpiresAt.Value <= DateTime.UtcNow)
            {
                return Result.Failure(AccessPermissionErrors.PermissionExpired);
            }
            if (permission.ExpiresAt != command.ExpiresAt.Value)
            {
                permission.ExpiresAt = command.ExpiresAt.Value;
                hasChanges = true;
            }
        }

        if (command.Note != null && permission.Note != command.Note.Trim())
        {
            permission.Note = command.Note.Trim();
            hasChanges = true;
        }

        if (hasChanges)
        {
            permission.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Публикация интеграционного события
            var @event = new AccessPermissionUpdatedEvent(
                permission.Id,
                permission.TenantId,
                permission.UserId,
                (PermissionTypeContract)(int)permission.Type,
                permission.ExpiresAt,
                permission.Note,
                permission.UpdatedAt);

            await _eventBus.PublishAsync(@event, cancellationToken);
        }

        return Result.Success();
    }
}

/// <summary>
/// Валидатор команды обновления разрешения доступа
/// </summary>
internal sealed class UpdateAccessPermissionCommandValidator : AbstractValidator<UpdateAccessPermissionCommand>
{
    public UpdateAccessPermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("Permission ID is required.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid permission type.")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future.")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Note must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}

