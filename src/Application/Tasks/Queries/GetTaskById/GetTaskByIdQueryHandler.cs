using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Tasks.Errors;
using SharedKernel;

namespace Application.Tasks.Queries.GetTaskById;

internal sealed class GetTaskByIdQueryHandler : IQueryHandler<GetTaskByIdQuery, TaskResponse>
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskByIdQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskResponse>> Handle(GetTaskByIdQuery query, CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdAsync(query.TaskId, cancellationToken);

        if (task is null)
        {
            return TaskErrors.NotFound(query.TaskId);
        }

        return new TaskResponse(
            task.Id,
            task.TenantId,
            task.ProjectId,
            task.Title,
            task.Description,
            task.AssignedUserId,
            task.DueDate,
            task.Status,
            task.Priority,
            task.CreatedAt);
    }
}

