using AutoMapper;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Identity.Application.Abstractions;
using Identity.Application.DTOs;
using Identity.Domain.Errors;

namespace Identity.Application.Queries.GetSession;

/// <summary>
/// Запрос получения сессии по ID
/// </summary>
public sealed record GetSessionQuery(Guid SessionId) : IQuery<SessionDto>;

/// <summary>
/// Обработчик запроса получения сессии
/// </summary>
internal sealed class GetSessionQueryHandler : IQueryHandler<GetSessionQuery, SessionDto>
{
    private readonly ISessionRepository _repository;
    private readonly IMapper _mapper;

    public GetSessionQueryHandler(ISessionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<SessionDto>> Handle(GetSessionQuery query, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(query.SessionId, cancellationToken);
        if (session == null)
        {
            return Result<SessionDto>.Failure(IdentityErrors.SessionNotFound(query.SessionId));
        }

        var dto = _mapper.Map<SessionDto>(session);
        return Result<SessionDto>.Success(dto);
    }
}

