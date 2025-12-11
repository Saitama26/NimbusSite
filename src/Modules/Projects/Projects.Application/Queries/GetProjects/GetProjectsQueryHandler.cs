using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Projects.Application.Abstractions;
using Projects.Application.DTOs;

namespace Projects.Application.Queries.GetProjects;

/// <summary>
/// Обработчик получения списка проектов.
/// </summary>
internal sealed class GetProjectsQueryHandler : IQueryHandler<GetProjectsQuery, IQueryable<ProjectListItemDto>>
{
    private readonly IProjectRepository _repository;
    private readonly IMapper _mapper;

    public GetProjectsQueryHandler(IProjectRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IQueryable<ProjectListItemDto>>> Handle(GetProjectsQuery query, CancellationToken cancellationToken)
    {
        var projects = await _repository.GetAllAsync(cancellationToken);
        var dtoQueryable = projects.ProjectTo<ProjectListItemDto>(_mapper.ConfigurationProvider);
        return Result<IQueryable<ProjectListItemDto>>.Success(dtoQueryable);
    }
}

