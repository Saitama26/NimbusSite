using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Tasks.Application.Abstractions;
using Tasks.Application.Queries.GetTasks;

namespace Tasks.Application.Queries.GetTasks;

/// <summary>
/// Обработчик запроса получения списка задач
/// </summary>
internal sealed class GetTasksQueryHandler : IQueryHandler<GetTasksQuery, IEnumerable<TaskListItemDto>>
{
    private readonly ITasksDbContext _dbContext;

    public GetTasksQueryHandler(ITasksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<TaskListItemDto>>> Handle(GetTasksQuery query, CancellationToken cancellationToken)
    {
        var tasks = await _dbContext.Tasks
            .AsNoTracking()
            .Where(t => t.TenantId == query.TenantId && t.Status != Tasks.Domain.Enums.TaskStatus.Deleted)
            .Select(t => new TaskListItemDto(
                t.Id,
                t.ProjectId,
                t.Title,
                t.Status,
                t.Priority,
                t.AssignedToUserId,
                t.DueDate,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<TaskListItemDto>>.Success(tasks);
    }
}

