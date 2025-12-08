using Application.Abstractions.Messaging;

namespace Application.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(Guid TaskId) : IQuery<TaskResponse> { }

