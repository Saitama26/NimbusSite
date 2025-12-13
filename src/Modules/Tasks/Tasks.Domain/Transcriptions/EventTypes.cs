namespace Tasks.Domain.Transcriptions;

/// <summary>
/// Короткие идентификаторы интеграционных событий Tasks
/// </summary>
public static class EventTypes
{
    public const string Created = "Task.Created";
    public const string Updated = "Task.Updated";
    public const string Deleted = "Task.Deleted";
    public const string StatusChanged = "Task.StatusChanged";
    public const string Assigned = "Task.Assigned";
}

