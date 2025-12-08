using Application.Abstractions.Messaging;

namespace Application.Tenants.Commands.CreateTenant;

public sealed record CreateTenantCommand(
    string Name,
    string ConnectionString
) : ICommand<Guid> {}

