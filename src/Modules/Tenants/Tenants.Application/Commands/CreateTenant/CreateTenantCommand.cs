using Common.Application.Abstractions.Messaging;
using Tenants.Application.Commands.CreateTenant;

namespace Tenants.Application.Commands.CreateTenant;

/// <summary>
/// Команда создания нового тенанта
/// </summary>
public sealed record CreateTenantCommand(
    string Name,
    string Subdomain,
    string? ConnectionString = null,
    string? Description = null,
    string? AdminEmail = null) : ICommand<CreateTenantResponse>;

