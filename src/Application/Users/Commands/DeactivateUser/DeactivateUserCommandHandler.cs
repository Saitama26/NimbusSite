using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Users;
using Domain.Users.Errors;
using Domain.Users.Events;
using SharedKernel;

namespace Application.Users.Commands.DeactivateUser;

internal sealed class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _context;

    public DeactivateUserCommandHandler(IUserRepository userRepository, IApplicationDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<Result<Guid>> Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return UserErrors.NotFound(command.UserId);
        }

        if (!user.IsActive)
        {
            return UserErrors.NotFound(command.UserId);
        }

        user.IsActive = false;
        user.AddEvent(new UserDeactivatedEvent(user.Id));

        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}

