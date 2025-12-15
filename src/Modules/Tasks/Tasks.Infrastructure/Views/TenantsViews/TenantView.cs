using System;

namespace Tasks.Infrastructure.Views.TenantsViews;

/// <summary>
/// EF-проекция на vw_Tenants (из базы Tenants).
/// </summary>
public sealed class TenantView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Subdomain { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

