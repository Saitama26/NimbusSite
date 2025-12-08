using SharedKernel;

namespace Domain.Tasks.Errors;

public static class TaskErrors
{
    public static Error NotFound(Guid taskId) =>
        new Error("Task.NotFound",
            $"The task with the id - {taskId} was not found",
            ErrorType.NotFound);

    public static Error AlreadyExists(string title) =>
        new Error("Task.AlreadyExists",
            $"The task with the title '{title}' already exists in this project",
            ErrorType.Conflict);

    public static Error AccessDenied() =>
        new Error("Task.AccessDenied",
            "You do not have access to this task.",
            ErrorType.Forbidden);

    public static Error AlreadyCompleted() =>
        new Error("Task.AlreadyCompleted",
            "The task is already completed and cannot be modified.",
            ErrorType.Failure);

    public static Error InvalidStatusTransition(string from, string to) =>
        new Error("Task.InvalidStatusTransition",
            $"Cannot transition task status from '{from}' to '{to}'.",
            ErrorType.Validation);

    public static Error DeadlineInPast() =>
        new Error("Task.DeadlineInPast",
            "The deadline cannot be set in the past.",
            ErrorType.Validation);
}

