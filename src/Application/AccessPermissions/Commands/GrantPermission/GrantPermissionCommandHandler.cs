using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Access;
using Domain.Access.Errors;
using Domain.Access.Events;
using Domain.Projects;
using Domain.Projects.Errors;
using Domain.Users;
using Domain.Users.Errors;
using Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.AccessPermissions.Commands.GrantPermission;

internal sealed class GrantPermissionCommandHandler : ICommandHandler<GrantPermissionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;

    public GrantPermissionCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        IProjectRepository projectRepository)
    {
        _context = context;
        _userRepository = userRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<Guid>> Handle(GrantPermissionCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound(command.UserId);
        }

        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project is null)
        {
            return ProjectErrors.NotFound(command.ProjectId);
        }

        // Проверяем, не существует ли уже активное разрешение
        var existingPermission = await _context.AccessPermissions
            .FirstOrDefaultAsync(
                ap => ap.UserId == command.UserId 
                    && ap.ProjectId == command.ProjectId 
                    && ap.RevokedAt == default(DateTime),
                cancellationToken);

        if (existingPermission is not null)
        {
            // Если разрешение уже есть, обновляем роль
            if (existingPermission.Role != command.Role)
            {
                existingPermission.Role = command.Role;
                existingPermission.AddEvent(new PermissionGrantedEvent(command.UserId, command.ProjectId, command.Role));
            }
            await _context.SaveChangesAsync(cancellationToken);
            return existingPermission.Id;
        }

        var permissionId = Guid.NewGuid();
        var permission = new AccessPermission
        {
            Id = permissionId,
            TenantId = command.TenantId,
            UserId = command.UserId,
            ProjectId = command.ProjectId,
            Role = command.Role,
            CreatedAt = DateTime.UtcNow,
            RevokedAt = default(DateTime) // Не отозвано
        };

        permission.AddEvent(new PermissionGrantedEvent(command.UserId, command.ProjectId, command.Role));

        _context.AccessPermissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);

        return permission.Id;
    }
}

