using AutoMapper;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Tasks.Application.Abstractions;
using Tasks.Application.DTOs;
using Tasks.Domain.Errors;

namespace Tasks.Application.Queries.GetTaskById;

/// <summary>
/// Обработчик запроса получения задачи по ID
/// </summary>
internal sealed class GetTaskByIdQueryHandler : IQueryHandler<GetTaskByIdQuery, TaskDto>
{
    private readonly ITaskRepository _repository;
    private readonly IMapper _mapper;

    public GetTaskByIdQueryHandler(ITaskRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery query, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(query.TaskId, cancellationToken);
        if (task == null)
        {
            return Result<TaskDto>.Failure(TaskErrors.NotFound(query.TaskId));
        }

        var taskDto = _mapper.Map<TaskDto>(task);
        return Result<TaskDto>.Success(taskDto);
    }
}

