using System;

namespace Tasks.Application.Abstractions.Views;

/// <summary>
/// DTO тенанта из view (Tenants).
/// </summary>
public sealed class TenantViewDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Status { get; init; }
    public string? Subdomain { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

