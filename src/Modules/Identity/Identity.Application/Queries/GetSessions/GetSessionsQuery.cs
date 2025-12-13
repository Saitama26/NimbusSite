using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Identity.Application.Abstractions;
using Identity.Application.DTOs;
using Identity.Domain.Entities;

namespace Identity.Application.Queries.GetSessions;

/// <summary>
/// Запрос получения списка сессий
/// </summary>
public sealed record GetSessionsQuery(Guid? UserId = null, Guid? TenantId = null) : IQuery<IQueryable<SessionDto>>;

/// <summary>
/// Обработчик запроса получения списка сессий
/// </summary>
internal sealed class GetSessionsQueryHandler : IQueryHandler<GetSessionsQuery, IQueryable<SessionDto>>
{
    private readonly ISessionRepository _repository;
    private readonly IMapper _mapper;

    public GetSessionsQueryHandler(ISessionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IQueryable<SessionDto>>> Handle(GetSessionsQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Session> sessions;

        if (query.UserId.HasValue)
        {
            sessions = await _repository.GetByUserIdAsync(query.UserId.Value, cancellationToken);
        }
        else if (query.TenantId.HasValue)
        {
            sessions = await _repository.GetByTenantIdAsync(query.TenantId.Value, cancellationToken);
        }
        else
        {
            // Если не указаны фильтры, возвращаем пустой результат
            // В реальном приложении может потребоваться ограничение доступа
            await System.Threading.Tasks.Task.CompletedTask;
            sessions = Enumerable.Empty<Session>().AsQueryable();
        }

        var dtoQueryable = sessions.ProjectTo<SessionDto>(_mapper.ConfigurationProvider);
        return Result<IQueryable<SessionDto>>.Success(dtoQueryable);
    }
}

