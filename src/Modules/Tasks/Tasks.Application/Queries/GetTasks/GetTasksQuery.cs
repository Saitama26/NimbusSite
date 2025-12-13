using Common.Application.Abstractions.Messaging;
using Tasks.Application.DTOs;

namespace Tasks.Application.Queries.GetTasks;

/// <summary>
/// Запрос получения списка задач
/// </summary>
public sealed record GetTasksQuery() : IQuery<IQueryable<TaskListItemDto>>;

