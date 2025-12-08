using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Access;
using Domain.Access.Errors;
using Domain.Access.Events;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.AccessPermissions.Commands.RevokePermission;

internal sealed class RevokePermissionCommandHandler : ICommandHandler<RevokePermissionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public RevokePermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(RevokePermissionCommand command, CancellationToken cancellationToken)
    {
        var permission = await _context.AccessPermissions
            .FirstOrDefaultAsync(
                ap => ap.UserId == command.UserId 
                    && ap.ProjectId == command.ProjectId 
                    && ap.RevokedAt == default(DateTime),
                cancellationToken);

        if (permission is null)
        {
            return AccessPermissionErrors.NotGranted(command.UserId, command.ProjectId);
        }

        if (permission.RevokedAt != default(DateTime))
        {
            return AccessPermissionErrors.AlreadyRevoked(command.UserId, command.ProjectId);
        }

        permission.RevokedAt = DateTime.UtcNow;
        permission.AddEvent(new PermissionRevokedEvent(command.UserId, command.ProjectId));

        await _context.SaveChangesAsync(cancellationToken);

        return permission.Id;
    }
}

