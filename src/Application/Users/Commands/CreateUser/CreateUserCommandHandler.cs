using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Auth;
using Domain.Users;
using Domain.Users.Errors;
using Domain.Users.Events;
using Domain.Tenants.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Users.Commands.CreateUser;

internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<CreateUserCommandHandler> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating user with email '{Email}' for tenant {TenantId}",
            command.Email,
            command.TenantId);

        // Validate tenant exists to avoid FK failures
        var tenantExists = await _context.Tenants
            .AnyAsync(t => t.Id == command.TenantId, cancellationToken);

        if (!tenantExists)
        {
            _logger.LogWarning(
                "Tenant {TenantId} not found. Aborting user creation for email '{Email}'",
                command.TenantId,
                command.Email);
            return TenantErrors.NotFound(command.TenantId);
        }

        if (await _context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            _logger.LogWarning(
                "User with email '{Email}' already exists",
                command.Email);
            return UserErrors.AlreadyExists(command.Email);
        }
        
        var userId = Guid.NewGuid();
        var user = new User() {
            Id = userId,
            TenantId = command.TenantId,
            UserName = command.UserName,
            Email = command.Email,
            PasswordHash = _passwordHasher.HashPassword(command.Password),
            Role = command.Role,
            CreatedAt = DateTime.UtcNow
            };

        user.AddEvent(new UserCreatedEvent(user.Id, user.UserName, user.Email));

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} with email '{Email}' created successfully for tenant {TenantId}",
            userId,
            command.Email,
            command.TenantId);

        return user.Id;
    }
}