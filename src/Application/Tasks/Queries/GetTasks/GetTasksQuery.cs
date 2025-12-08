using Application.Abstractions.Messaging;

namespace Application.Tasks.Queries.GetTasks;

public sealed record GetTasksQuery(
    Guid? ProjectId = null,
    Guid? AssignedUserId = null,
    Guid? TenantId = null
) : IQuery<IReadOnlyList<TaskResponse>> { }

