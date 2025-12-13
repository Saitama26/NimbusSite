using Common.Domain.Results;

namespace Tasks.Domain.Errors;

/// <summary>
/// Ошибки домена Tasks
/// </summary>
public static class TaskErrors
{
    public static Error NotFound(Guid taskId) =>
        Error.NotFound("Task.NotFound", $"Task with ID {taskId} was not found.");

    public static Error TitleEmpty =>
        Error.Validation("Task.TitleEmpty", "Task title cannot be empty.");

    public static Error TitleTooLong =>
        Error.Validation("Task.TitleTooLong", "Task title cannot exceed 500 characters.");

    public static Error AlreadyDeleted =>
        Error.Conflict("Task.AlreadyDeleted", "Task is already deleted.");

    public static Error CannotChangeStatus =>
        Error.Validation("Task.CannotChangeStatus", "Cannot change task status.");

    public static Error InvalidDueDate =>
        Error.Validation("Task.InvalidDueDate", "Due date cannot be in the past.");

    public static Error ProjectNotFound(Guid projectId) =>
        Error.NotFound("Task.ProjectNotFound", $"Project with ID {projectId} was not found.");

    public static Error UserNotFound(Guid userId) =>
        Error.NotFound("Task.UserNotFound", $"User with ID {userId} was not found.");

    public static Error CannotAssignToDeletedUser =>
        Error.Validation("Task.CannotAssignToDeletedUser", "Cannot assign task to a deleted user.");
}

