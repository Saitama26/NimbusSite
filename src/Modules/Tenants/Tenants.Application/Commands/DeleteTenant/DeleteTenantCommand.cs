using Common.Application.Abstractions.Messaging;

namespace Tenants.Application.Commands.DeleteTenant;

/// <summary>
/// Команда удаления тенанта (soft delete)
/// </summary>
public sealed record DeleteTenantCommand(Guid TenantId) : ICommand;

