using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Users;
using Domain.Users.Errors;
using Domain.Users.Events;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Users.Commands.ChangeUserRole;

internal sealed class ChangeUserRoleCommandHandler : ICommandHandler<ChangeUserRoleCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ChangeUserRoleCommandHandler> _logger;

    public ChangeUserRoleCommandHandler(
        IUserRepository userRepository,
        IApplicationDbContext context,
        ILogger<ChangeUserRoleCommandHandler> logger)
    {
        _userRepository = userRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(ChangeUserRoleCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Changing role for user {UserId} to {NewRole}",
            command.UserId,
            command.NewRole);

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning(
                "User {UserId} not found",
                command.UserId);
            return UserErrors.NotFound(command.UserId);
        }

        if (user.Role == command.NewRole)
        {
            _logger.LogInformation(
                "User {UserId} already has role {Role}",
                command.UserId,
                command.NewRole);
            return user.Id; // Роль уже установлена
        }

        var oldRole = user.Role;
        user.Role = command.NewRole;
        user.AddEvent(new UserRoleChangedEvent(user.Id, oldRole, command.NewRole));

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} role changed from {OldRole} to {NewRole}",
            command.UserId,
            oldRole,
            command.NewRole);

        return user.Id;
    }
}

