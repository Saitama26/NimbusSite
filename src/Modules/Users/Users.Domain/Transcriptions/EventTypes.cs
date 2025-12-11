namespace Users.Domain.Transcriptions;

/// <summary>
/// Короткие идентификаторы интеграционных событий Users.
/// </summary>
public static class EventTypes
{
    public const string Created = "User.Created";
    public const string Updated = "User.Updated";
    public const string Deleted = "User.Deleted";
    public const string StatusChanged = "User.StatusChanged";
    public const string RoleChanged = "User.RoleChanged";
}

