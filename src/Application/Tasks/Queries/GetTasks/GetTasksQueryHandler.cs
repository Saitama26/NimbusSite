using Application.Abstractions.Repositories;
using Application.Abstractions.Messaging;
using Domain.Tasks;
using SharedKernel;

namespace Application.Tasks.Queries.GetTasks;

internal sealed class GetTasksQueryHandler : IQueryHandler<GetTasksQuery, IReadOnlyList<TaskResponse>>
{
    private readonly ITaskRepository _taskRepository;

    public GetTasksQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<IReadOnlyList<TaskResponse>>> Handle(GetTasksQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProjectTask> tasks;

        if (query.ProjectId.HasValue)
        {
            tasks = await _taskRepository.GetByProjectIdAsync(query.ProjectId.Value, cancellationToken);
        }
        else if (query.AssignedUserId.HasValue)
        {
            tasks = await _taskRepository.GetByAssigneeIdAsync(query.AssignedUserId.Value, cancellationToken);
        }
        else if (query.TenantId.HasValue)
        {
            tasks = await _taskRepository.GetByTenantIdAsync(query.TenantId.Value, cancellationToken);
        }
        else
        {
            tasks = await _taskRepository.GetAllAsync(cancellationToken);
        }

        var response = tasks.Select(task => new TaskResponse(
            task.Id,
            task.TenantId,
            task.ProjectId,
            task.Title,
            task.Description,
            task.AssignedUserId,
            task.DueDate,
            task.Status,
            task.Priority,
            task.CreatedAt
        )).ToList();

        return response;
    }
}

