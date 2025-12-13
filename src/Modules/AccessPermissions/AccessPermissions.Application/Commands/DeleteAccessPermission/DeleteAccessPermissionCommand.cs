using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Events;
using Common.Domain.Results;
using FluentValidation;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Domain.Errors;
using Contracts.AccessPermissions.Events;

namespace AccessPermissions.Application.Commands.DeleteAccessPermission;

/// <summary>
/// Команда удаления разрешения доступа
/// </summary>
public sealed record DeleteAccessPermissionCommand(Guid PermissionId) : ICommand;

/// <summary>
/// Обработчик команды удаления разрешения доступа
/// </summary>
internal sealed class DeleteAccessPermissionCommandHandler : ICommandHandler<DeleteAccessPermissionCommand>
{
    private readonly IAccessPermissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public DeleteAccessPermissionCommandHandler(
        IAccessPermissionRepository repository,
        IUnitOfWork unitOfWork,
        IEventBus eventBus)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(DeleteAccessPermissionCommand command, CancellationToken cancellationToken)
    {
        var permission = await _repository.GetByIdAsync(command.PermissionId, cancellationToken);
        if (permission == null)
        {
            return Result.Failure(AccessPermissionErrors.NotFound(command.PermissionId));
        }

        var events = new List<IDomainEvent>
        {
            new AccessPermissionDeletedEvent(
                permission.Id,
                permission.TenantId,
                permission.UserId,
                DateTime.UtcNow)
        };

        await _repository.DeleteAsync(permission, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(events, cancellationToken);

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

