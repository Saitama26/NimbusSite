using Common.Application.Abstractions.Messaging;
using Tenants.Domain.Enums;

namespace Tenants.Application.Commands.ChangeTenantStatus;

/// <summary>
/// Команда изменения статуса тенанта
/// </summary>
public sealed record ChangeTenantStatusCommand(
    Guid TenantId,
    TenantStatus NewStatus) : ICommand;

