using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Tasks.Application.Abstractions;
using Tasks.Application.Queries.GetTaskById;
using Tasks.Domain.Errors;

namespace Tasks.Application.Queries.GetTaskById;

/// <summary>
/// Обработчик запроса получения задачи по ID
/// </summary>
internal sealed class GetTaskByIdQueryHandler : IQueryHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly ITasksDbContext _dbContext;

    public GetTaskByIdQueryHandler(ITasksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery query, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == query.TaskId && t.TenantId == query.TenantId && t.Status != Tasks.Domain.Enums.TaskStatus.Deleted, cancellationToken);
        if (task == null)
        {
            return Result<TaskDto>.Failure(TaskErrors.NotFound(query.TaskId));
        }

        var dto = new TaskDto(
            task.Id,
            task.TenantId,
            task.ProjectId,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.AssignedToUserId,
            task.CreatedByUserId,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt,
            task.StartedAt,
            task.CompletedAt);

        return Result<TaskDto>.Success(dto);
    }
}

