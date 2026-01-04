using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Identity.Application.Abstractions;
using Identity.Application.Queries.GetSession;
using Microsoft.EntityFrameworkCore;

namespace Identity.Application.Queries.GetSessions;

/// <summary>
/// Запрос получения списка сессий
/// </summary>
public sealed record GetSessionsQuery(Guid? UserId = null, int? TenantId = null) : IQuery<IEnumerable<SessionDto>>;

/// <summary>
/// Обработчик запроса получения списка сессий
/// </summary>
internal sealed class GetSessionsQueryHandler : IQueryHandler<GetSessionsQuery, IEnumerable<SessionDto>>
{
    private readonly IIdentityDbContext _dbContext;

    public GetSessionsQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<SessionDto>>> Handle(GetSessionsQuery query, CancellationToken cancellationToken)
    {
        var sessionsQuery = _dbContext.Sessions.AsNoTracking();

        if (query.UserId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.UserId == query.UserId.Value);
        }

        if (query.TenantId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.TenantId == query.TenantId.Value);
        }

        // Если не указаны фильтры, возвращаем пустой результат
        if (!query.UserId.HasValue && !query.TenantId.HasValue)
        {
            return Result<IEnumerable<SessionDto>>.Success(Enumerable.Empty<SessionDto>());
        }

        var sessions = await sessionsQuery
            .Select(s => new SessionDto(
                s.Id,
                s.UserId,
                s.TenantId,
                s.RefreshTokenId,
                s.Status,
                s.IpAddress,
                s.UserAgent,
                s.LastActivityAt,
                s.ExpiresAt,
                s.ClosedAt,
                s.CloseReason,
                s.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<SessionDto>>.Success(sessions);
    }
}

