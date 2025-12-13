using Common.Application.Abstractions.Messaging;
using Tasks.Application.DTOs;

namespace Tasks.Application.Queries.GetTaskById;

/// <summary>
/// Запрос получения задачи по ID
/// </summary>
public sealed record GetTaskByIdQuery(Guid TaskId) : IQuery<TaskDto>;

