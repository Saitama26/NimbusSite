using AutoMapper;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Projects.Application.Abstractions;
using Projects.Application.DTOs;
using Projects.Domain.Errors;

namespace Projects.Application.Queries.GetProjectById;

/// <summary>
/// Обработчик получения проекта по ID.
/// </summary>
internal sealed class GetProjectByIdQueryHandler : IQueryHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IProjectRepository _repository;
    private readonly IMapper _mapper;

    public GetProjectByIdQueryHandler(IProjectRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken)
    {
        var project = await _repository.GetByIdAsync(query.ProjectId, cancellationToken);
        if (project == null)
        {
            return Result<ProjectDto>.Failure(ProjectErrors.NotFound(query.ProjectId));
        }

        var dto = _mapper.Map<ProjectDto>(project);
        return Result<ProjectDto>.Success(dto);
    }
}

