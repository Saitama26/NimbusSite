using Common.Application.Abstractions.Messaging;

namespace Tenants.Application.Commands.UpdateTenant;

/// <summary>
/// Команда обновления тенанта
/// </summary>
public sealed record UpdateTenantCommand(
    Guid TenantId,
    string? Name = null,
    string? Description = null,
    string? AdminEmail = null) : ICommand;

