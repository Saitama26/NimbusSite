using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Domain.Entities;
using AccessPermissions.Domain.Enums;
using AccessPermissions.Domain.Errors;
using Contracts.AccessPermissions.Events;
using Contracts.AccessPermissions;

namespace AccessPermissions.Application.Commands.CreateAccessPermission;

/// <summary>
/// Команда создания разрешения доступа
/// </summary>
public sealed record CreateAccessPermissionCommand(
    Guid TenantId,
    Guid UserId,
    PermissionScope Scope,
    PermissionAction Action,
    PermissionType Type,
    Guid CreatedByUserId,
    Guid? ProjectId = null,
    Guid? TaskId = null,
    DateTime? ExpiresAt = null,
    string? Note = null) : ICommand<CreateAccessPermissionResponse>;

/// <summary>
/// Ответ при создании разрешения доступа
/// </summary>
public sealed record CreateAccessPermissionResponse(Guid PermissionId);

/// <summary>
/// Обработчик команды создания разрешения доступа
/// </summary>
internal sealed class CreateAccessPermissionCommandHandler : ICommandHandler<CreateAccessPermissionCommand, CreateAccessPermissionResponse>
{
    private readonly IAccessPermissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public CreateAccessPermissionCommandHandler(
        IAccessPermissionRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result<CreateAccessPermissionResponse>> Handle(CreateAccessPermissionCommand command, CancellationToken cancellationToken)
    {
        // Валидация scope
        if (command.Scope == PermissionScope.Project && !command.ProjectId.HasValue)
        {
            return Result<CreateAccessPermissionResponse>.Failure(AccessPermissionErrors.InvalidScope);
        }

        if (command.Scope == PermissionScope.Task && (!command.ProjectId.HasValue || !command.TaskId.HasValue))
        {
            return Result<CreateAccessPermissionResponse>.Failure(AccessPermissionErrors.InvalidScope);
        }

        // Проверка на дубликат
        var exists = await _repository.ExistsAsync(
            command.TenantId,
            command.UserId,
            command.Scope,
            command.Action,
            command.Type,
            command.ProjectId,
            command.TaskId,
            cancellationToken);

        if (exists)
        {
            return Result<CreateAccessPermissionResponse>.Failure(AccessPermissionErrors.PermissionAlreadyExists);
        }

        // Создание разрешения
        var permission = new AccessPermission
        {
            TenantId = command.TenantId,
            UserId = command.UserId,
            ProjectId = command.ProjectId,
            TaskId = command.TaskId,
            Scope = command.Scope,
            Action = command.Action,
            Type = command.Type,
            CreatedByUserId = command.CreatedByUserId,
            ExpiresAt = command.ExpiresAt,
            Note = command.Note?.Trim()
        };

        var events = new List<IDomainEvent>
        {
            new AccessPermissionCreatedEvent(
                permission.Id,
                permission.TenantId,
                permission.UserId,
                (PermissionScopeContract)(int)permission.Scope,
                (PermissionActionContract)(int)permission.Action,
                (PermissionTypeContract)(int)permission.Type,
                permission.CreatedByUserId,
                permission.ProjectId,
                permission.TaskId,
                permission.ExpiresAt,
                permission.Note,
                permission.CreatedAt)
        };

        await _repository.AddAsync(permission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

        return Result<CreateAccessPermissionResponse>.Success(new CreateAccessPermissionResponse(permission.Id));
    }
}

/// <summary>
/// Валидатор команды создания разрешения доступа
/// </summary>
internal sealed class CreateAccessPermissionCommandValidator : AbstractValidator<CreateAccessPermissionCommand>
{
    public CreateAccessPermissionCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created by user ID is required.");

        RuleFor(x => x.Scope)
            .IsInEnum().WithMessage("Invalid permission scope.");

        RuleFor(x => x.Action)
            .IsInEnum().WithMessage("Invalid permission action.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid permission type.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required when scope is Project or Task.")
            .When(x => x.Scope == PermissionScope.Project || x.Scope == PermissionScope.Task);

        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("Task ID is required when scope is Task.")
            .When(x => x.Scope == PermissionScope.Task);

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future.")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x.Note)
            .MaximumLength(500).WithMessage("Note must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Note));
    }
}

