using Application.Abstractions.Messaging;

namespace Application.Tenants.Commands.UpdateTenant;

public sealed record UpdateTenantCommand(
    Guid TenantId,
    string? Name = null,
    string? ConnectionString = null
) : ICommand<Guid> {}

