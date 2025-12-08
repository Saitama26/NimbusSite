using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants;
using Domain.Tenants.Errors;
using Domain.Tenants.Events;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.Commands.UpdateTenant;

internal sealed class UpdateTenantCommandHandler : ICommandHandler<UpdateTenantCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public UpdateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(UpdateTenantCommand command, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == command.TenantId, cancellationToken);

        if (tenant is null)
        {
            return TenantErrors.NotFound(command.TenantId);
        }

        bool hasChanges = false;

        if (command.Name is not null && tenant.Name != command.Name)
        {
            // Проверяем, не существует ли другой тенант с таким именем
            if (await _context.Tenants.AnyAsync(t => t.Name == command.Name && t.Id != command.TenantId, cancellationToken))
            {
                return TenantErrors.AlreadyExists(command.Name);
            }

            tenant.Name = command.Name;
            hasChanges = true;
        }

        if (command.ConnectionString is not null && tenant.ConnectionString != command.ConnectionString)
        {
            tenant.ConnectionString = command.ConnectionString;
            hasChanges = true;
        }

        if (hasChanges)
        {
            tenant.AddEvent(new TenantUpdatedEvent(tenant.Id, tenant.Name));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return tenant.Id;
    }
}

