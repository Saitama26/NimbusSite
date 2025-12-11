using Common.Domain.Results;

namespace Projects.Domain.Errors;

/// <summary>
/// Ошибки домена Projects.
/// </summary>
public static class ProjectErrors
{
    public static Error NotFound(Guid projectId) =>
        Error.NotFound("Project.NotFound", $"Project with ID {projectId} was not found.");

    public static Error NameAlreadyExists(string name) =>
        Error.Conflict("Project.NameAlreadyExists", $"Project with name '{name}' already exists.");

    public static Error NameEmpty =>
        Error.Validation("Project.NameEmpty", "Project name cannot be empty.");

    public static Error AlreadyDeleted =>
        Error.Conflict("Project.AlreadyDeleted", "Project is already deleted.");

    public static Error CannotChangeStatus =>
        Error.Validation("Project.CannotChangeStatus", "Cannot change project status.");
}

