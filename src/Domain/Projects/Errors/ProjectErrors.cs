using SharedKernel;

namespace Domain.Projects.Errors;

public static class ProjectErrors
{
    public static Error NotFound(Guid projectId) =>
        new Error("Project.NotFound",
            $"The project with the id - {projectId} was not found",
            ErrorType.NotFound);

    public static Error AlreadyExists(string name) =>
        new Error("Project.AlreadyExists",
            $"The project with the name '{name}' already exists",
            ErrorType.Conflict);

    public static Error AlreadyClosed() =>
        new Error("Project.AlreadyClosed",
            "The project is already closed and cannot be modified.",
            ErrorType.Failure);

    public static Error InvalidDateRange() =>
        new Error("Project.InvalidDateRange",
            "The end date must be after the start date.",
            ErrorType.Validation);
}