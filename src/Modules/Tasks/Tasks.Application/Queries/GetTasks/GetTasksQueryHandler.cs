using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Tasks.Application.Abstractions;
using Tasks.Application.DTOs;

namespace Tasks.Application.Queries.GetTasks;

/// <summary>
/// Обработчик запроса получения списка задач
/// </summary>
internal sealed class GetTasksQueryHandler : IQueryHandler<GetTasksQuery, IQueryable<TaskListItemDto>>
{
    private readonly ITaskRepository _repository;
    private readonly IMapper _mapper;

    public GetTasksQueryHandler(ITaskRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IQueryable<TaskListItemDto>>> Handle(GetTasksQuery query, CancellationToken cancellationToken)
    {
        var tasks = await _repository.GetAllAsync(cancellationToken);
        var dtoQueryable = tasks.ProjectTo<TaskListItemDto>(_mapper.ConfigurationProvider);
        return Result<IQueryable<TaskListItemDto>>.Success(dtoQueryable);
    }
}

