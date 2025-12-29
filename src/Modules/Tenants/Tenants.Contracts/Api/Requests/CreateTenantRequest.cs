namespace Tenants.Contracts.Api.Requests;

/// <summary>
/// Запрос на создание тенанта (публичный API)
/// ConnectionString генерируется автоматически на основе TenantInt после создания тенанта
/// Если требуется кастомная connection string (например, для внешней БД), используйте PUT /api/Tenants/{tenantInt}/connection-string
/// </summary>
public sealed record CreateTenantRequest(
    string Name,
    string? Description = null);

