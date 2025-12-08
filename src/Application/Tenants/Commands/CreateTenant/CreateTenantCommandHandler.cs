using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Domain.Tenants.Errors;
using Domain.Tenants.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Tenants.Commands.CreateTenant;

internal sealed class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateTenantCommandHandler> _logger;

    public CreateTenantCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateTenantCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating tenant with name '{TenantName}'",
            command.Name);

        // Проверяем, существует ли тенант с таким именем
        if (await _context.Tenants.AnyAsync(t => t.Name == command.Name, cancellationToken))
        {
            _logger.LogWarning(
                "Tenant with name '{TenantName}' already exists",
                command.Name);
            return TenantErrors.AlreadyExists(command.Name);
        }

        var tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            TenantId = tenantId, // TenantId равен Id для тенанта
            Name = command.Name,
            ConnectionString = command.ConnectionString,
            CreatedAt = DateTime.UtcNow
        };

        tenant.AddEvent(new TenantCreatedEvent(tenant.Id, tenant.Name));

        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tenant {TenantId} with name '{TenantName}' created successfully",
            tenantId,
            command.Name);

        return tenant.Id;
    }
}

