using Common.Application.Abstractions.Messaging;

namespace Tenants.Application.Commands.UpdateTenantConnectionString;

/// <summary>
/// Команда обновления строки подключения тенанта
/// </summary>
public sealed record UpdateTenantConnectionStringCommand(
    Guid TenantId,
    string ConnectionString) : ICommand;

