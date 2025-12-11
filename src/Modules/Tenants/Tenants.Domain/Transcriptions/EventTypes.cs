namespace Tenants.Domain.Transcriptions;

/// <summary>
/// Короткие идентификаторы интеграционных событий Tenants.
/// </summary>
public static class EventTypes
{
    public const string Created = "Tenant.Created";
    public const string Updated = "Tenant.Updated";
    public const string Deleted = "Tenant.Deleted";
    public const string StatusChanged = "Tenant.StatusChanged";
    public const string ConnectionInfoChanged = "Tenant.ConnectionInfoChanged";
}

