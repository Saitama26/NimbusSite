using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Domain.Enums;
using AccessPermissions.Domain.Errors;
using Contracts.AccessPermissions.Events;
using Contracts.AccessPermissions;

namespace AccessPermissions.Application.Commands.UpdateAccessPermission;

/// <summary>
/// Команда обновления разрешения доступа
/// </summary>
public sealed record UpdateAccessPermissionCommand(
    Guid PermissionId,
    PermissionType? Type = null,
    DateTime? ExpiresAt = null,
    string? Note = null) : ICommand;

/// <summary>
/// Обработчик команды обновления разрешения доступа
/// </summary>
internal sealed class UpdateAccessPermissionCommandHandler : ICommandHandler<UpdateAccessPermissionCommand>
{
    private readonly IAccessPermissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public UpdateAccessPermissionCommandHandler(
        IAccessPermissionRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(UpdateAccessPermissionCommand command, CancellationToken cancellationToken)
    {
        var permission = await _repository.GetByIdAsync(command.PermissionId, cancellationToken);
        if (permission == null)
        {
            return Result.Failure(AccessPermissionErrors.NotFound(command.PermissionId));
        }

        // Проверка на истечение
        if (!permission.IsValid)
        {
            return Result.Failure(AccessPermissionErrors.PermissionExpired);
        }

        // Обновление полей
        if (command.Type.HasValue)
        {
            permission.Type = command.Type.Value;
        }

        if (command.ExpiresAt.HasValue)
        {
            if (command.ExpiresAt.Value <= DateTime.UtcNow)
            {
                return Result.Failure(AccessPermissionErrors.PermissionExpired);
            }
            permission.ExpiresAt = command.ExpiresAt.Value;
        }

        if (command.Note != null)
        {
            permission.Note = command.Note.Trim();
        }

        permission.UpdatedAt = DateTime.UtcNow;

        var events = new List<IDomainEvent>
        {
            new AccessPermissionUpdatedEvent(
                permission.Id,
                permission.TenantId,
                permission.UserId,
                (PermissionTypeContract)(int)permission.Type,
                permission.ExpiresAt,
                permission.Note,
                permission.UpdatedAt ?? DateTime.UtcNow)
        };

        await _repository.UpdateAsync(permission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

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

